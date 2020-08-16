using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using ProductCatalog.Domain.Entities.Base;
using ProductCatalog.Domain.Interfaces.UoW;
using ProductCatalog.Infra.CrossCutting.Bus;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.Persistance.UoW
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        readonly ProductsCatalogDbContext _context;
        readonly IMediatorHandler _mediatorHandler;

        public UnitOfWork(ProductsCatalogDbContext context, IMediatorHandler mediatorHandler)
        {
            _context = context;
            _mediatorHandler = mediatorHandler;
        }

        public async Task<bool> Commit(bool autoDetectChangesOff)
        {
            if (autoDetectChangesOff)
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

            return await Commit();
        }

        public async Task<bool> Commit()
        {
            var success = await _context.SaveChangesAsync() > 0;

            if(success)
                await _mediatorHandler.PublishDomainEvents(_context).ConfigureAwait(false);

            return success;
        }
    }

    public static class MediatorExtension
    {
        public static async Task PublishDomainEvents<T>(this IMediatorHandler mediator, T context) where T : DbContext
        {
            var domainEntities = context.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            var tasks = domainEvents
                .Select(async (domainEvent) => {
                    await mediator.PublishEvent(domainEvent);
                });

            await Task.WhenAll(tasks);
        }
    }
}

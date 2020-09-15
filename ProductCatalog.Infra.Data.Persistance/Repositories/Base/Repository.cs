using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Interfaces.Repositories.Base;
using ProductCatalog.Domain.Interfaces.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductCatalog.Infra.Data.Persistance.Repositories.Base
{
    public abstract class Repository<T> : IRepository<T> where T : class 
    {
        readonly protected IUnitOfWork _unitOfWork;
        readonly protected ProductsCatalogDbContext _context;
        readonly DbSet<T> _entity;
        public bool AsNoTracking { get; set; }

        public Repository(ProductsCatalogDbContext context, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _entity = _context.Set<T>();
        }

        public void Add(T entity)
        {
            _entity.Add(entity);
        }

        public void Update(T entity)
        {
            _entity.Update(entity);
        }

        public void UpdateRange(T[] entities)
        {
            _entity.UpdateRange(entities);
        }
        public T Search(params int[] id)
        {
            var entity = _entity.Find(id[0]);
            _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public void Delete(params int[] id)
        {
            Delete(Search(id));
        }

        public void Delete(T entity)
        {
            _entity.Remove(entity);
        }

        public void Delete(T[] entities)
        {
            _entity.RemoveRange(entities);
        }

        public IQueryable<T> List()
        {
            if (AsNoTracking)
                return _entity.AsNoTracking();
            else
                return _entity;
        }
    }
}

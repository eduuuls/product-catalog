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

        public Repository(ProductsCatalogDbContext context, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _entity = _context.Set<T>();
        }

        public void Add(T entidade)
        {
            _entity.Add(entidade);
        }

        public void Update(T entidade)
        {
            _entity.Update(entidade);
        }

        public T Search(params int[] id)
        {
            var entidade = _entity.Find(id[0]);
            _context.Entry(entidade).State = EntityState.Detached;
            return entidade;
        }

        public void Delete(params int[] id)
        {
            Delete(Search(id));
        }

        public void Delete(T entidade)
        {
            _entity.Remove(entidade);
        }

        public IQueryable<T> List(bool asNoTracking = true)
        {
            if (asNoTracking)
                return _entity.AsNoTracking();
            else
                return _entity;
        }
    }
}

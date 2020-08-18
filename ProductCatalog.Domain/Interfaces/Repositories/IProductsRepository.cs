using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.Repositories.Base;
using ProductCatalog.Domain.Interfaces.UoW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.Repositories
{
    public interface IProductsRepository : IRepository<Product>
    {
        IUnitOfWork UnitOfWork { get; }
        Task<Product> GetByKey(Guid categoryId, string externalId, DataProvider dataProvider);
        Task<Product> GetById(Guid id);
    }
}

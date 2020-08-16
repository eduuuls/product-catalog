using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Repositories.Base;
using ProductCatalog.Domain.Interfaces.UoW;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Interfaces.Repositories
{
    public interface IProductsRepository : IRepository<Product>
    {
        IUnitOfWork UnitOfWork { get; }
    }
}

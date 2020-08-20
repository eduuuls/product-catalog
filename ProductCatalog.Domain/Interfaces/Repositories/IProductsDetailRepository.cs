using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.Repositories
{
    public interface IProductsDetailRepository : IRepository<ProductDetail>
    {
        Task<ProductDetail> GetByProductId(Guid productId);
    }
}

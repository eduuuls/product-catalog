using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Domain.Interfaces.UoW;
using ProductCatalog.Infra.Data.Persistance.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.Persistance.Repositories
{
    public class ProductsDetailRepository : Repository<ProductDetail>, IProductsDetailRepository
    {
        public ProductsDetailRepository(ProductsCatalogDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {

        }
        
        public async Task<ProductDetail> GetByProductId(Guid productId)
        {
            return await List().FirstOrDefaultAsync(c => c.ProductId == productId);
        }
    }
}

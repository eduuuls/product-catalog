using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Domain.Interfaces.UoW;
using ProductCatalog.Infra.Data.Persistance.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.Persistance.Repositories
{
    public class ProductsReviewsRepository : Repository<ProductReview>, IProductsReviewsRepository
    {
        public IUnitOfWork UnitOfWork => _unitOfWork;
        public ProductsReviewsRepository(ProductsCatalogDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {

        }

        public async Task<ProductReview> GetByKey(Guid productId, string externalId)
        {
            var productReview = await List().FirstOrDefaultAsync(r => r.ProductId == productId
                                                                    && r.ExternalId == externalId);

            return productReview;
        }

        public async Task<List<ProductReview>> GetByProductId(Guid productId)
        {
            return await List().Where(r => r.ProductId == productId).ToListAsync();
        }
    }
}

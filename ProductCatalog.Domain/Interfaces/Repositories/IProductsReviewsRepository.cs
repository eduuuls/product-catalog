using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Repositories.Base;
using ProductCatalog.Domain.Interfaces.UoW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.Repositories
{
    public interface IProductsReviewsRepository : IRepository<ProductReview>
    {
        IUnitOfWork UnitOfWork { get; }
        Task<ProductReview> GetByKey(Guid productId, string externalId);
    }
}

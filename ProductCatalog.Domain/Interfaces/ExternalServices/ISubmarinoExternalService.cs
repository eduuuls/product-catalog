using ProductCatalog.Domain.DTO;
using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.ExternalServices
{
    public interface ISubmarinoExternalService
    {
        Task<List<CategoryDTO>> GetProductCategories();
        List<ProductDTO> GetProductsByCategory(Guid categoryId, string categoryUrl);
        Task<List<ProductReviewDTO>> GetProductReviews(RequestB2WReviewDTO requestB2WReview);
    }
}

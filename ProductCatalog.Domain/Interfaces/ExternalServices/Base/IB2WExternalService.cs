using HtmlAgilityPack;
using ProductCatalog.Domain.DTO;
using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.ExternalServices
{
    public interface IB2WExternalService
    {
        Task<CategoryDTO[]> GetCategoriesData();
        Task<ProductDTO[]> GetProductsData(Guid categoryId, string categoryUrl);
        Task<List<ProductReviewDTO>> GetProductReviews(RequestB2WReviewDTO requestB2WReview);
        Task<CategoryDTO[]> GetCategoryAdditionalData(CategoryDTO categoryDTO);
        Task<ProductDTO> GetProductAdditionalData(Guid categoryId, ProductDTO productDTO);
    }
}

using ProductCatalog.Application.ViewModels;

namespace ProductCatalog.Application.Interfaces
{
    public interface IProductReviewsJob
    {
        void ImportProductReviews(ProductViewModel productViewModel);
    }
}

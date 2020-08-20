using ProductCatalog.Application.ViewModels;
using System.Threading.Tasks;

namespace ProductCatalog.Application.Interfaces
{
    public interface IProductReviewsJob
    {
        Task ImportProductReviews(ProductViewModel productViewModel);
    }
}

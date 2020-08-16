using ProductCatalog.Application.ViewModels;

namespace ProductCatalog.Application.Interfaces
{
    public interface IProductJob
    {
        void ImportProducts(CategoryViewModel categoryViewModel);
    }
}

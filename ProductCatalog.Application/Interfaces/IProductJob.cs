using ProductCatalog.Application.ViewModels;

namespace ProductCatalog.Application.Interfaces
{
    public interface IProductJob
    {
        void ImportProductsJob(CategoryViewModel categoryViewModel);
    }
}

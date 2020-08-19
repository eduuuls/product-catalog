using ProductCatalog.Application.ViewModels;
using System.Threading.Tasks;

namespace ProductCatalog.Application.Interfaces
{
    public interface IProductJob
    {
        Task ImportProducts(CategoryViewModel categoryViewModel);
    }
}

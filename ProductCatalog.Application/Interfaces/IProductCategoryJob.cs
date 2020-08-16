using ProductCatalog.Domain.Enums;
using System.Threading.Tasks;

namespace ProductCatalog.Application.Interfaces
{
    public interface IProductCategoryJob
    {
        Task ImportCategoriesJob(DataProvider dataProvider);
    }
}

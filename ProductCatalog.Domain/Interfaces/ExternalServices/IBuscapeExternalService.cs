using ProductCatalog.Domain.DTO;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.ExternalServices
{
    public interface IBuscapeExternalService
    {
        Task<ProductDTO[]> SearchProducts(string search);
    }
}

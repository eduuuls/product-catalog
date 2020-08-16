using ProductCatalog.Domain.DTO;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.ExternalServices
{
    public interface IMercadoLivreExternalService
    {
        Task<ProductDTO[]> SearchProducts(string search);
    }
}

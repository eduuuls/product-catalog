using System.Threading.Tasks;

namespace ProductCatalog.Application.Interfaces
{
    public interface IMessagePublisher<T>
    {
        Task Publish<U>(U obj);
    }
}

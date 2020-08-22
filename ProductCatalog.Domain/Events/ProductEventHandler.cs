using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ProductCatalog.Domain.Events
{
    public class ProductEventHandler : INotificationHandler<ProductDataChangedEvent>
    {
        public Task Handle(ProductDataChangedEvent message, CancellationToken cancellationToken)
        {
        
            return Task.CompletedTask;
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Domain.Events
{
    public class CategoryEventHandler: INotificationHandler<CategoryCreatedEvent>
    {
        public Task Handle(CategoryCreatedEvent message, CancellationToken cancellationToken)
        {
            

            return Task.CompletedTask;
        }
    }
}

using FluentValidation.Results;
using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Events.Base;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.CrossCutting.Bus
{
    public interface IMediatorHandler
    {
        Task PublishEvent<T>(T @event) where T : Event;
        Task<ValidationResult> SendCommand<T>(T command) where T : Command;
    }
}

using FluentValidation.Results;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Events.Base;
using ProductCatalog.Infra.CrossCutting.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.CrossCutting.Bus
{
    public sealed class MessageBus : IMediatorHandler
    {
        private readonly IMediator _mediator;
        private readonly ServiceBusConfiguration _serviceBusConfiguration;

        public MessageBus(IMediator mediator, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _mediator = mediator;
            _serviceBusConfiguration = serviceBusConfig.Value;
        }

        public async Task PublishEvent<T>(T @event) where T : Event
        {
            if (@event.Destination ==  MessageDestination.ServiceBusMessage)
                await SendMessage<T>(@event, @event.TopicKey, @event.MessageType);
            else
                await _mediator.Publish(@event);
        }

        public async Task<ValidationResult> SendCommand<T>(T command) where T : Command
        {
            if (command.Destination == MessageDestination.ServiceBusMessage)
            {
                await SendMessage<T>(command, command.TopicKey, command.MessageType);

                return command.ValidationResult;
            }
            else
                return await _mediator.Send(command);
        }

        private async Task SendMessage<T>(object obj, string topicKey, string messageType)
        {
            var topic = _serviceBusConfiguration.Topics.First(t => t.Key == topicKey);
            var topicClient = new TopicClient(connectionString: _serviceBusConfiguration.ConnectionString, entityPath: topic.Name);

            var messageBody = JsonConvert.SerializeObject(obj);

            var message = new Message(body: Encoding.UTF8.GetBytes(messageBody.Compress()));

            message.UserProperties["messageType"] = messageType;

            await topicClient.SendAsync(message);
        }
    }
}

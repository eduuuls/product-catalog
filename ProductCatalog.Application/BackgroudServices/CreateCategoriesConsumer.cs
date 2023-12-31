using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Infra.CrossCutting.Bus;
using ProductCatalog.Infra.CrossCutting.Extensions;

namespace ProductCatalog.Application.BackgroundServices
{
    public class CreateCategoriesConsumer : BackgroundService
    {
        private readonly IMediatorHandler _mediatorHandler;
        private readonly ISubscriptionClient _subscriptionClient;
        public CreateCategoriesConsumer(IServiceScopeFactory serviceScopeFactory, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _mediatorHandler = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMediatorHandler>();
            
            var categoryTopic = serviceBusConfig.Value.Topics.First(t => t.Key == "CategoryTopic");

            _subscriptionClient = new SubscriptionClient(connectionString: serviceBusConfig.Value.ConnectionString,
                                                topicPath: categoryTopic.Name,
                                                    subscriptionName: categoryTopic.Subscriptions.First(s => s.Key == "CreationSubscription").Name);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriptionClient.RegisterMessageHandler((message, token) =>
            {
                var messageBody = Encoding.UTF8.GetString(message.Body).Decompress();

                var createNewCategoryCommand = JsonConvert.DeserializeObject<CreateNewCategoryCommand>(messageBody);

                var commandTask = _mediatorHandler.SendCommand(createNewCategoryCommand);

                commandTask.Wait();

                if (commandTask.Result.IsValid)
                    return _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                else
                {
                    var errors = JsonConvert.SerializeObject(commandTask.Result.Errors);

                    return _subscriptionClient.DeadLetterAsync(message.SystemProperties.LockToken, deadLetterReason: errors);
                }

            }, new MessageHandlerOptions(args => Task.CompletedTask)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            });

            return Task.CompletedTask;
        }
    }
}

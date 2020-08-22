using System;
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
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Interfaces.Queries;
using ProductCatalog.Infra.CrossCutting.Bus;
using ProductCatalog.Infra.CrossCutting.Extensions;

namespace ProductCatalog.Application.BackgroundServices
{
    public class ProductsMergeDoneEventConsumer : BackgroundService
    {
        private readonly IMediatorHandler _mediatorHandler;
        private readonly ISubscriptionClient _subscriptionClient;
        private readonly IProductsQuery _productsQuery;
        public ProductsMergeDoneEventConsumer(IServiceScopeFactory serviceScopeFactory, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _mediatorHandler = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMediatorHandler>();
            _productsQuery = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IProductsQuery>();

            var categoryTopic = serviceBusConfig.Value.Topics.First(t => t.Key == "EventHubTopic");

            _subscriptionClient = new SubscriptionClient(connectionString: serviceBusConfig.Value.ConnectionString,
                                                topicPath: categoryTopic.Name,
                                                    subscriptionName: categoryTopic.Subscriptions.First(s => s.Key == "ProductsMergeDoneSubscription").Name);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriptionClient.RegisterMessageHandler((message, token) =>
            {
                var messageBody = Encoding.UTF8.GetString(message.Body).Decompress();

                var productsMergeDoneEvent = JsonConvert.DeserializeObject<ProductsMergeDoneEvent>(messageBody);

                if (productsMergeDoneEvent != null)
                {
                    var product = _productsQuery.GetProductFullDataById(productsMergeDoneEvent.ProductId).Result;

                    var command = new UpdateProductsQueryCommand(product.Id, product.Category, product.Name, product.BarCode, product.Brand, product.Manufacturer, 
                                                                    product.Model, product.RelevancePoints, product.ReferenceModel, product.Supplier, product.OtherSpecs, 
                                                                        product.ImageUrl, product.DataProvider, product.Reviews);

                    var commandTask = _mediatorHandler.SendCommand(command);

                    commandTask.Wait();
                }

                return _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            }, new MessageHandlerOptions(args => Task.CompletedTask)
            {
                MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                AutoComplete = false,
                MaxConcurrentCalls = 1
            });

            return Task.CompletedTask;
        }
    }
}

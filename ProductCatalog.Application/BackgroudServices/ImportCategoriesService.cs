using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Enums;
using Microsoft.Extensions.Options;
using ProductCatalog.Application.Configuration;
using System.Linq;
using ProductCatalog.Application.Interfaces;

namespace ProductCatalog.Application.BackgroundServices
{
    public class ImportCategoriesService : BackgroundService
    {
        private readonly ISubscriptionClient _subscriptionClient;
        private readonly IProductCategoryJob _productCategoryJobs;
        public ImportCategoriesService(IServiceScopeFactory serviceScopeFactory, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _productCategoryJobs = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IProductCategoryJob>();

            var categoryTopic = serviceBusConfig.Value.Topics.First(t => t.Key == "CategoryTopic");

            _subscriptionClient = new SubscriptionClient(connectionString: serviceBusConfig.Value.ConnectionString,
                                                topicPath: categoryTopic.Name,
                                                    subscriptionName: categoryTopic.Subscriptions.First(s=> s.Key == "ImportSubscription").Name);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriptionClient.RegisterMessageHandler(async (message, token) =>
            {
                var command = Encoding.UTF8.GetString(message.Body);

                var dataProvider = (DataProvider)Enum.Parse(typeof(DataProvider), command);

                Task task = _productCategoryJobs.ImportCategories(dataProvider);

                task.Wait();

                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            }, new MessageHandlerOptions(args => Task.CompletedTask)
            {
                MaxAutoRenewDuration = TimeSpan.FromMinutes(60),
                AutoComplete = false,
                MaxConcurrentCalls = 5
            });

            return Task.CompletedTask;
        }
    }
}

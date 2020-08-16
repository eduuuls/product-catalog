using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Linq;
using Newtonsoft.Json;
using ProductCatalog.Domain.Events;
using AutoMapper;
using ProductCatalog.Infra.CrossCutting.Extensions;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.ViewModels;

namespace ProductCatalog.Application.BackgroundServices
{
    public class ImportProductsService : BackgroundService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ImportProductsService> _logger;
        private readonly ISubscriptionClient _subscriptionClient;
        private readonly IProductJob _productJob;
        public ImportProductsService(IServiceScopeFactory serviceScopeFactory, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _mapper = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMapper>();
            _logger = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<ImportProductsService>>();
            _productJob = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IProductJob>();

            var topic = serviceBusConfig.Value.Topics.First(t => t.Key == "CategoryTopic");

            _subscriptionClient = new SubscriptionClient(connectionString: serviceBusConfig.Value.ConnectionString,
                                                topicPath: topic.Name,
                                                    subscriptionName: topic.Subscriptions.First(s => s.Key == "UpdatedSubscription").Name);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ImportProductsService] Registering MessageHandler...");
            _subscriptionClient.RegisterMessageHandler((message, token) =>
            {
                _logger.LogInformation($"[ImportProductsService] Processing CategoryUpdatedEvent message...");

                var messageBody = Encoding.UTF8.GetString(message.Body).Decompress();

                var categoryUpdatedEvent = JsonConvert.DeserializeObject<CategoryUpdatedEvent>(messageBody);
                
                if (categoryUpdatedEvent.IsActive)
                {
                    _logger.LogInformation($"[ImportProductsService] The category [{categoryUpdatedEvent.Name}] was activated...");

                    var categoryViewModel = _mapper.Map<CategoryViewModel>(categoryUpdatedEvent);

                    _productJob.ImportProducts(categoryViewModel);
                }

                return _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            }, new MessageHandlerOptions(args => Task.CompletedTask)
            {
                MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                AutoComplete = false,
                MaxConcurrentCalls = 1
            }) ;

            return Task.CompletedTask;
        }
    }
}

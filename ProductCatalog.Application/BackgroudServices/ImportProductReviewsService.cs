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
    public class ImportProductReviewsService : BackgroundService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ImportProductReviewsService> _logger;
        private readonly ISubscriptionClient _subscriptionClient;
        private readonly IProductReviewsJob _productReviewsJob;
        public ImportProductReviewsService(IServiceScopeFactory serviceScopeFactory, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _mapper = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMapper>();
            _logger = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<ImportProductReviewsService>>();
            _productReviewsJob = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IProductReviewsJob>();

            var topic = serviceBusConfig.Value.Topics.First(t => t.Key == "ProductTopic");

            _subscriptionClient = new SubscriptionClient(connectionString: serviceBusConfig.Value.ConnectionString,
                                                topicPath: topic.Name,
                                                    subscriptionName: topic.Subscriptions.First(s => s.Key == "CreatedSubscription").Name);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ImportProductReviewsService] Registering MessageHandler...");
            _subscriptionClient.RegisterMessageHandler((message, token) =>
            {
                _logger.LogInformation($"[ImportProductReviewsService] Processing ProductCreatedEvent message...");

                var messageBody = Encoding.UTF8.GetString(message.Body).Decompress();

                var productCreatedEvent = JsonConvert.DeserializeObject<ProductCreatedEvent>(messageBody);
                
                var productViewModel = _mapper.Map<ProductViewModel>(productCreatedEvent);

                _productReviewsJob.ImportProductReviews(productViewModel);
                
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

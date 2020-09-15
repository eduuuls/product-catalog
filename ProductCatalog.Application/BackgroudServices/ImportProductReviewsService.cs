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

            var topic = serviceBusConfig.Value.Topics.First(t => t.Key == "EventHubTopic");

            _subscriptionClient = new SubscriptionClient(connectionString: serviceBusConfig.Value.ConnectionString,
                                                topicPath: topic.Name,
                                                    subscriptionName: topic.Subscriptions.First(s => s.Key == "ProductCreatedUpdatedSubscription").Name);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ImportProductReviewsService] Registering MessageHandler...");
            _subscriptionClient.RegisterMessageHandler((message, token) =>
            {
                _logger.LogInformation($"[ImportProductReviewsService] Starting Import Product Reviews Process...");

                var messageBody = Encoding.UTF8.GetString(message.Body).Decompress();

                var productCreatedUpdatedEvent = JsonConvert.DeserializeObject<ProductCreatedUpdatedEvent>(messageBody);
                
                var productViewModel = _mapper.Map<ProductViewModel>(productCreatedUpdatedEvent);

                var task = _productReviewsJob.ImportProductReviews(productViewModel);
                
                task.Wait();

                _logger.LogInformation($"[ImportProductReviewsService] Process End.");
                return _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            }, new MessageHandlerOptions(args => Task.CompletedTask)
            {
                MaxAutoRenewDuration = TimeSpan.FromMinutes(120),
                AutoComplete = false,
                MaxConcurrentCalls = 5
            }) ;

            return Task.CompletedTask;
        }
    }
}

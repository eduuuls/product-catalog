using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Infra.CrossCutting.Bus;
using ProductCatalog.Infra.CrossCutting.Extensions;

namespace ProductCatalog.Application.BackgroundServices
{
    public class CreateCategoryConsumerService : BackgroundService
    {
        private readonly IMediatorHandler _mediatorHandler;
        private readonly ILogger<CreateCategoryConsumerService> _logger;
        private readonly ISubscriptionClient _subscriptionClient;
        public CreateCategoryConsumerService(IServiceScopeFactory serviceScopeFactory, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _mediatorHandler = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMediatorHandler>();
            _logger = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<CreateCategoryConsumerService>>();
            
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

                var createNewCategoriesCommand = JsonConvert.DeserializeObject<CreateNewCategoriesCommand>(messageBody);

                var commandTask = _mediatorHandler.SendCommand(createNewCategoriesCommand);

                commandTask.Wait();

                return _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            }, new MessageHandlerOptions(args => Task.CompletedTask)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            });

            return Task.CompletedTask;
        }
    }
}

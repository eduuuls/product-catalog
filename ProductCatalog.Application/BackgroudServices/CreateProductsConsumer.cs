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
    public class CreateProductsConsumer : BackgroundService
    {
        private readonly IMediatorHandler _mediatorHandler;
        private readonly ISubscriptionClient _subscriptionClient;
        public CreateProductsConsumer(IServiceScopeFactory serviceScopeFactory, IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            _mediatorHandler = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMediatorHandler>();
            
            var categoryTopic = serviceBusConfig.Value.Topics.First(t => t.Key == "ProductTopic");

            _subscriptionClient = new SubscriptionClient(connectionString: serviceBusConfig.Value.ConnectionString,
                                                topicPath: categoryTopic.Name,
                                                    subscriptionName: categoryTopic.Subscriptions.First(s => s.Key == "CreationSubscription").Name);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriptionClient.RegisterMessageHandler((message, token) =>
            {
                var messageBody = Encoding.UTF8.GetString(message.Body).Decompress();

                var createNewProductCommand = JsonConvert.DeserializeObject<CreateNewProductCommand>(messageBody);

                var commandTask = _mediatorHandler.SendCommand(createNewProductCommand);

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
                MaxAutoRenewDuration = TimeSpan.FromMinutes(20),
                AutoComplete = false,
                MaxConcurrentCalls = 1
            });

            return Task.CompletedTask;
        }
    }
}

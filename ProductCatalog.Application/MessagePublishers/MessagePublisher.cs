using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProductCatalog.Application.Configuration;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Infra.CrossCutting.Extensions;

namespace ProductCatalog.Application.MessagePublishers
{
    public class MessagePublisher<T>: IMessagePublisher<T>
    {
        private readonly ITopicClient _topicClient;

        public MessagePublisher(IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            Topic topic = new Topic();

            if(typeof(T).Name == "CategoryJob")
                topic = serviceBusConfig.Value.Topics.First(t => t.Key == "CategoryTopic");
            else if (typeof(T).Name == "ProductJob")
                topic = serviceBusConfig.Value.Topics.First(t => t.Key == "ProductTopic");
            else if (typeof(T).Name == "ProductReviewsJob")
                topic = serviceBusConfig.Value.Topics.First(t => t.Key == "ReviewsTopic");

            _topicClient = new TopicClient(connectionString: serviceBusConfig.Value.ConnectionString, entityPath: topic.Name);
            _topicClient.OperationTimeout = TimeSpan.FromSeconds(60);
        }
        public Task Publish<U>(U obj)
        {
            var messageBody = JsonConvert.SerializeObject(obj);
            
            var message = new Message(body: Encoding.UTF8.GetBytes(messageBody.Compress()));
            
            message.UserProperties["messageType"] = typeof(U).Name;

            return _topicClient.SendAsync(message);
        }
    }
}

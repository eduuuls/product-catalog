using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Events.Base;
using System;

namespace ProductCatalog.Domain.Events
{
    public class ProductsMergeDoneEvent : Event
    {
        public Guid ProductId { get; set; }
        public ProductsMergeDoneEvent(Guid productId)
            : base(MessageDestination.ServiceBusMessage, typeof(ProductsMergeDoneEvent).Name)
        {
            ProductId = productId;
            TopicKey = "EventHubTopic";
        }
    }
}

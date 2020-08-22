using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Events.Base;
using System;

namespace ProductCatalog.Domain.Events
{
    public class ProductDataChangedEvent : Event
    {
        public Guid ProductId { get; set; }
        public ProductDataChangedEvent(Guid productId)
            : base(MessageDestination.ServiceBusMessage, typeof(ProductDataChangedEvent).Name)
        {
            ProductId = productId;
            TopicKey = "EventHubTopic";
        }

        public Guid Id { get; protected set; }
    }
}

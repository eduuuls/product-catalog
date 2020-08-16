using MediatR;
using ProductCatalog.Domain.Enums;
using System;

namespace ProductCatalog.Domain.Events.Base
{
    public abstract class Event : Message, INotification
    {
        public DateTime Timestamp { get; private set; }
        
        protected Event(MessageDestination destination, string messageType)
            : base(destination, messageType)
        {
            Timestamp = DateTime.Now;
        }
    }
}

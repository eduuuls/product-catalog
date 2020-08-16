using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ProductCatalog.Domain
{
    public abstract class Message
    {
        public MessageDestination Destination { get; protected set; }
        public Guid AggregateId { get; protected set; }
        public string MessageType { get; protected set; }
        public string TopicKey { get; protected set; }
        protected Message(MessageDestination destination, string messageType)
        {
            Destination = destination;
            MessageType = messageType;
        }
    }
}

using MediatR;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Events.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Events
{
    public class CategoryUpdatedEvent : Event
    {
        public CategoryUpdatedEvent(Guid id, string name, string subType, string description, string url,
                                            string imageUrl, bool isActive, int numberOfProducts, DataProvider dataProvider)
            : base(MessageDestination.ServiceBusMessage, typeof(CategoryUpdatedEvent).Name)
        {
            Id = id;
            Name = name;
            SubType = subType;
            Description = description;
            Url = url;
            ImageUrl = imageUrl;
            IsActive = isActive;
            NumberOfProducts = numberOfProducts;
            DataProvider = dataProvider;
            TopicKey = "EventHubTopic";
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SubType { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfProducts { get; set; }
        public DataProvider DataProvider { get; set; }
    }
}

using MediatR;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Events.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Events
{
    public class ProductCreatedEvent : Event
    {
        public ProductCreatedEvent(Guid id, Guid categoryId, string externalId, string name, string description,
                                        string url, string imageUrl, DataProvider dataProvider, ProductDetail detail,
                                            IEnumerable<ProductReview> reviews)
            : base(MessageDestination.ServiceBusMessage, typeof(ProductCreatedEvent).Name)
        {
            Id = id;
            CategoryId = categoryId;
            ExternalId = externalId;
            Name = name;
            Description = description;
            Url = url;
            ImageUrl = imageUrl;
            DataProvider = dataProvider;
            Detail = detail;
            Reviews = reviews;
            TopicKey = "ProductTopic";
        }

        public Guid Id { get; protected set; }
        public Guid CategoryId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DataProvider DataProvider { get; set; }
        public ProductDetail Detail { get; set; }
        public IEnumerable<ProductReview> Reviews { get; set; }
    }
}

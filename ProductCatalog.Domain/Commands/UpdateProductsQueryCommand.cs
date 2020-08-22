using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProductCatalog.Domain.Commands
{
    public class UpdateProductsQueryCommand: ProductCommand
    {
        public UpdateProductsQueryCommand(Guid id, Category category, string name, string barCode, string brand, 
                                            string manufacturer, string model, int relevancePoints, string referenceModel, 
                                                string supplier, string otherSpecs, string imageUrl, DataProvider dataProvider, 
                                                    IEnumerable<ProductReview> reviews)
               : base(MessageDestination.ServiceBusMessage, typeof(UpdateProductsQueryCommand).Name)
        {
            Id = id;
            Category = category;
            Name = name;
            ImageUrl = imageUrl;
            DataProvider = dataProvider;
            BarCode = barCode;
            RelevancePoints = relevancePoints;
            Brand = brand;
            Manufacturer = manufacturer;
            Model = model;
            ReferenceModel = referenceModel;
            Supplier = supplier;
            OtherSpecs = otherSpecs;
            Reviews = reviews;
            TopicKey = "ProductQueryTopic";
        }

        public override bool IsValid()
        {
            ValidationResult = new UpdateProductsQueryCommandValidation().Validate(this);

            return base.IsValid();
        }
    }
}

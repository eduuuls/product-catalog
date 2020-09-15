using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProductCatalog.Domain.Commands
{
    public class MergeProductCommand : ProductCommand
    {
        public MergeProductCommand(Guid id, Guid categoryId, string name, string barCode, string brand,
                                            string manufacturer, string model, int relevancePoints, string referenceModel,
                                                string supplier, string otherSpecs, string imageUrl, DataProvider dataProvider,
                                                    IEnumerable<ProductReview> reviews)
               : base(MessageDestination.DomainMessage, typeof(MergeProductCommand).Name)
        {
            Id = id;
            CategoryId = categoryId;
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
        }

        public override bool IsValid()
        {
            ValidationResult = new MergeProductCommandValidation().Validate(this);

            return base.IsValid();
        }
    }
}

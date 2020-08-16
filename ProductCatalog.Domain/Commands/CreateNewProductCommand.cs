using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands
{
    public class CreateNewProductCommand : ProductCommand
    {
        public CreateNewProductCommand(Guid categoryId, string externalId, string name, string description,
                                        string barCode, string code, string brand, string manufacturer, string model,
                                            string referenceModel, string supplier, string otherSpecs, string url, string imageUrl, 
                                                DataProvider dataProvider, IEnumerable<ProductReview> reviews)
            : base(MessageDestination.DomainMessage, typeof(CreateNewProductCommand).Name)
        {
            CategoryId = categoryId;
            ExternalId = externalId;
            Name = name;
            Description = description;
            Url = url;
            ImageUrl = imageUrl;
            DataProvider = dataProvider;
            BarCode = barCode;
            Code = code;
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
            ValidationResult = new CreateNewProductCommandValidation().Validate(this);

            return base.IsValid();
        }
    }
}

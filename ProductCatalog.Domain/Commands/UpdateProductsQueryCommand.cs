using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProductCatalog.Domain.Commands
{
    public class UpdateProductsQueryCommand: ProductCommand
    {
        public int Recomendations { get; set; }
        public int Disrecomendations { get; set; }
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
        public UpdateProductsQueryCommand(Guid id, Category category, string name, string barCode, string brand, 
                                            string manufacturer, string model, int relevancePoints, string referenceModel, 
                                                string supplier, string otherSpecs, string imageUrl, DataProvider dataProvider, 
                                                    int recomendations, int disrecomendations, int fiveStars, int fourStars, int threeStars,
                                                        int twoStars, int oneStar, IEnumerable<ProductReview> reviews)
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
            Recomendations = recomendations;
            Disrecomendations = disrecomendations;
            FiveStars = fiveStars;
            FourStars = fourStars;
            ThreeStars = threeStars;
            TwoStars = twoStars;
            OneStar = oneStar;
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

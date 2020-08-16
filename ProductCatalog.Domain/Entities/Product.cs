using ProductCatalog.Domain.Entities.Base;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Entities
{
    public class Product : Entity
    {
        public Product()
        {

        }
        public Product(Guid id, Guid categoryId, string externalId, string name, string description,
                        string barCode, string code, string brand, string manufacturer, string model,
                            string referenceModel, string supplier, string otherSpecs, string url, 
                                string imageUrl, DataProvider dataProvider, IEnumerable<ProductReview> reviews)
        {
            Id = id;
            CategoryId = categoryId;
            ExternalId = externalId;
            Name = name;
            Description = description;
            Url = url;
            ImageUrl = imageUrl;
            DataProvider = dataProvider;
            Detail = new ProductDetail()
            {
                BarCode = barCode,
                Code = code,
                Brand = brand,
                Manufacturer = manufacturer,
                Model = model,
                ReferenceModel = referenceModel,
                Supplier = supplier,
                OtherSpecs = otherSpecs,
            };

            Reviews = reviews;
        }

        public Guid CategoryId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public DataProvider DataProvider { get; set; }
        public ProductDetail Detail { get; set; }
        public IEnumerable<ProductReview> Reviews { get; set; }
        public Category ProductCategory { get; set; }
    }
}

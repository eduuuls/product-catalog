using ProductCatalog.Domain.Entities.Base;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Entities
{
    public class Category: Entity
    {
        public Category()
        {

        }

        public Category(Guid id, string name, string subType, string description, string url, string imageUrl, 
                            bool isActive, int numberOfProducts, DataProvider dataProvider)
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
        }

        public string Name { get; set; }
        public string SubType { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfProducts { get; set; }
        public DataProvider DataProvider { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}

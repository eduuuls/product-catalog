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

        public Category(Guid id, string name, string description, string imageUrl, bool isActive, 
                            int numberOfProducts, DataProvider dataProvider, IEnumerable<CategoryLink> links)
        {
            Id = id;
            Name = name;
            Description = description;
            Links = links;
            ImageUrl = imageUrl;
            IsActive = isActive;
            NumberOfProducts = numberOfProducts;
            DataProvider = dataProvider;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfProducts { get; set; }
        public DataProvider DataProvider { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<CategoryLink> Links { get; set; }
    }
}

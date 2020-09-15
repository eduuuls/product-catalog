using ProductCatalog.Domain.Entities.Base;
using System;
using System.Text.Json.Serialization;

namespace ProductCatalog.Domain.Entities
{
    public class CategoryLink : Entity
    {
        public CategoryLink()
        {

        }

        public CategoryLink(Guid id, Guid categoryId, bool isActive, string description, string url, int numberOfProducts)
        {
            Id = id;
            CategoryId = categoryId;
            Description = description;
            Url = url;
            NumberOfProducts = numberOfProducts;
            IsActive = isActive;
        }

        public Guid CategoryId { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfProducts { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }
        
    }
}

using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProductCatalog.Domain.DTO.Query
{
    public class ProductQueryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public DataProvider DataProvider { get; set; }
        public int RelevancePoints { get; set; }
        public string BarCode { get; set; }
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string Model { get; set; }
        public string ReferenceModel { get; set; }
        public string OtherSpecs { get; set; }
        public IEnumerable<ProductReview> Reviews { get; set; }
        public Category Category { get; set; }
    }
}

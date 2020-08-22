using Newtonsoft.Json;
using ProductCatalog.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Entities
{
    public class ProductDetail: Entity
    {
        public Guid ProductId { get; set; }
        public string Code { get; set; }
        public string BarCode { get; set; }
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string Model { get; set; }
        public string ReferenceModel { get; set; }
        public string OtherSpecs { get; set; }
        public string MergedProductsId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
    }
}

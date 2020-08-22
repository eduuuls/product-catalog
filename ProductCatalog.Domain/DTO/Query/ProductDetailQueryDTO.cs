using System;

namespace ProductCatalog.Domain.DTO.Query
{
    public class ProductDetailQueryDTO
    {
        public Guid Id { get; set; }
        public string BarCode { get; set; }
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string Model { get; set; }
        public string ReferenceModel { get; set; }
        public string OtherSpecs { get; set; }
    }
}

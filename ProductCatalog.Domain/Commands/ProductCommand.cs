using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands
{
    public abstract class ProductCommand: Command
    {
        public ProductCommand(MessageDestination destination, string type)
            : base(destination, type)
        {

        }
        public Guid Id { get; protected set; }
        public Guid CategoryId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public DataProvider DataProvider { get; set; }
        public string Code { get; set; }
        public string BarCode { get; set; }
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string Model { get; set; }
        public string ReferenceModel { get; set; }
        public string OtherSpecs { get; set; }
        public IEnumerable<ProductReview> Reviews { get; set; }
        public Category ProductCategory { get; set; }
    }
}

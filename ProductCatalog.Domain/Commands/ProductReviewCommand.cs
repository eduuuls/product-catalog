using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Enums;
using System;

namespace ProductCatalog.Domain.Commands
{
    public class ProductReviewCommand : Command
    {
        public ProductReviewCommand(MessageDestination destination, string type)
            : base(destination, type)
        {
         
        }

        public Guid Id { get; protected set; }
        public Guid ProductId { get; set; }
        public string ExternalId { get; set; }
        public string Reviewer { get; set; }
        public DateTime? Date { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public short? Stars { get; set; }
        public string Result { get; set; }
        public bool? IsRecommended { get; set; }
    }
}

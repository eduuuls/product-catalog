using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands
{
    public class AddProductReviewsCommand : Command
    {
        public Guid ProductId { get; protected set; }
        public List<ProductReview> Reviews { get; protected set; }

        public AddProductReviewsCommand(Guid productId, List<ProductReview> reviews)
            : base(MessageDestination.DomainMessage, typeof(AddProductReviewsCommand).Name)
        {
            ProductId = productId;
            Reviews = reviews;
        }

        public override bool IsValid()
        {
            Reviews.ForEach(r =>
            {
                ValidationResult = new AddProductReviewsCommandValidation().Validate(r);
            });

            return base.IsValid();
        }
    }
}

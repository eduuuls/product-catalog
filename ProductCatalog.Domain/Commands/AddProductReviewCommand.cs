using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Enums;
using System;

namespace ProductCatalog.Domain.Commands
{
    public class AddProductReviewCommand : ProductReviewCommand
    {
        public AddProductReviewCommand(Guid productId, string externalId, string reviewer, DateTime? date, 
                                            string title, string text, short? stars, string result, bool? isRecommended)
            : base(MessageDestination.DomainMessage, typeof(AddProductReviewCommand).Name)
        {
            ProductId = productId;
            ExternalId = externalId;
            Reviewer = reviewer;
            Date = date;
            Title = title;
            Text = text;
            Stars = stars;
            Result = result;
            IsRecommended = isRecommended;
        }

        public override bool IsValid()
        {
            ValidationResult = new AddProductReviewCommandValidation().Validate(this);

            return base.IsValid();
        }
    }
}

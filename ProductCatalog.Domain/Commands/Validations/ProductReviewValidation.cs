using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public abstract class ProductReviewValidation<T> : AbstractValidator<T> where T : ProductReviewCommand
    {
        protected void ValidateExternalId()
        {
            RuleFor(c => c.ExternalId)
                .NotNull()
                .NotEqual(string.Empty)
                .MaximumLength(50);
        }

        protected void ValidateProductId()
        {
            RuleFor(c => c.ProductId)
                .NotEqual(Guid.Empty);
        }
        protected void ValidateTitle()
        {
            RuleFor(c => c.Title)
                .NotEqual(string.Empty)
                .NotNull()
                .MaximumLength(500);
        }
        protected void ValidateText()
        {
            RuleFor(c => c.Text)
                .NotEqual(string.Empty)
                .NotNull()
                .MaximumLength(2000);
        }
    }
}

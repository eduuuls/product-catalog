using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public abstract class ProductValidation<T> : AbstractValidator<T> where T : ProductCommand
    {
        protected void ValidateId()
        {
            RuleFor(c => c.Id)
                .NotEqual(Guid.Empty);
        }

        protected void ValidateName()
        {
            RuleFor(c => c.Name)
                .NotNull()
                .NotEqual(string.Empty)
                .MaximumLength(200);
        }

        protected void ValidateExternalId()
        {
            RuleFor(c => c.ExternalId)
                .NotNull()
                .NotEqual(string.Empty)
                .MaximumLength(50);
        }

        protected void ValidateDescription()
        {
            RuleFor(c => c.Description)
                .MaximumLength(300);
        }

        protected void ValidateUrl()
        {
            RuleFor(c => c.Url)
                .NotNull()
                .NotEqual(string.Empty)
                .MaximumLength(1000);
        }

        protected void ValidateDetailSpecs()
        {
            RuleFor(c => c.OtherSpecs)
                .MaximumLength(8000);
        }

        protected void ValidateImageUrl()
        {
            RuleFor(c => c.ImageUrl)
                .NotNull()
                .NotEqual(string.Empty)
                .MaximumLength(1000);
        }
    }
}

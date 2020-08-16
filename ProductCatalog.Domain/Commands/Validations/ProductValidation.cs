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
                .NotEqual(string.Empty);
        }

        protected void ValidateUrl()
        {
            RuleFor(c => c.Url)
                .NotEqual(string.Empty);
        }
    }
}

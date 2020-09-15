using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public abstract class CategoryValidation<T> : AbstractValidator<T> where T : CategoryCommand
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

        protected void ValidateLinks()
        {
            RuleFor(c => c.Links)
                .NotEmpty();
        }
    }
}

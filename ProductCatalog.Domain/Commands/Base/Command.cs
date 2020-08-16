using FluentValidation.Results;
using MediatR;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Base
{
    public abstract class Command : Message, IRequest<ValidationResult>
    {
        protected Command(MessageDestination destination, string messageType)
            : base(destination, messageType)
        {
            ValidationResult = new ValidationResult();
        }
        public ValidationResult ValidationResult { get; set; }
        public virtual bool IsValid()
        {
            return ValidationResult.IsValid;
        }
    }
}

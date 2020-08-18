using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProductCatalog.Domain.Commands
{
    public class AddProductReviewsCommand : Command
    {
        public List<AddProductReviewCommand> Commands { get; protected set; }

        public AddProductReviewsCommand(List<AddProductReviewCommand> commands)
            : base(MessageDestination.DomainMessage, typeof(AddProductReviewsCommand).Name)
        {
            Commands = commands;
        }

        public override bool IsValid()
        {
            Commands.ForEach(c =>
            {
                ValidationResult = new AddProductReviewCommandValidation().Validate(c);
            });

            return base.IsValid();
        }
    }
}

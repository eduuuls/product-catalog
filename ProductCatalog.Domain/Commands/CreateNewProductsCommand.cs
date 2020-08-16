using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands
{
    public class CreateNewProductsCommand : Command
    {
        public List<CreateNewProductCommand> Commands { get; protected set; }

        public CreateNewProductsCommand(List<CreateNewProductCommand> commands)
            : base(MessageDestination.DomainMessage, typeof(CreateNewProductsCommand).Name)
        {
            Commands = commands;
        }

        public override bool IsValid()
        {
            Commands.ForEach(c =>
            {
                ValidationResult = new CreateNewProductCommandValidation().Validate(c);
            });

            return base.IsValid();
        }
    }
}

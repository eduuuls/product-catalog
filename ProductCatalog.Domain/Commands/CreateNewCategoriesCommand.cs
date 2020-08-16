using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands
{
    public class CreateNewCategoriesCommand : Command
    {
        public List<CreateNewCategoryCommand> Commands { get; protected set; }

        public CreateNewCategoriesCommand(List<CreateNewCategoryCommand> commands)
            : base(MessageDestination.DomainMessage, typeof(CreateNewCategoriesCommand).Name)
        {
            Commands = commands;
        }

        public override bool IsValid()
        {
            Commands.ForEach(c =>
            {
                ValidationResult = new CreateNewCategoryCommandValidation().Validate(c);
            });

            return base.IsValid();
        }
    }
}

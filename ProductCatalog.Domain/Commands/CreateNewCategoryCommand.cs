using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Commands.Validations;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands
{
    public class CreateNewCategoryCommand : CategoryCommand
    {
        public CreateNewCategoryCommand()
            : base(MessageDestination.DomainMessage, typeof(CreateNewCategoryCommand).Name)
        {

        }
        public CreateNewCategoryCommand(string name, string description, string imageUrl, bool isActive, int numberOfProducts, 
                                                DataProvider dataProvider, IEnumerable<CategoryLink> links)
            : base(MessageDestination.DomainMessage, typeof(CreateNewCategoryCommand).Name)
        {
            Name = name;
            Description = description;
            ImageUrl = imageUrl;
            IsActive = isActive;
            NumberOfProducts = numberOfProducts;
            DataProvider = dataProvider;
            Links = links;
        }

        public override bool IsValid()
        {
            ValidationResult = new CreateNewCategoryCommandValidation().Validate(this);

            return base.IsValid();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public class CreateNewCategoryCommandValidation: CategoryValidation<CreateNewCategoryCommand>
    {
        public CreateNewCategoryCommandValidation()
        {
            ValidateName();
            ValidateLinks();
        }
    }
}

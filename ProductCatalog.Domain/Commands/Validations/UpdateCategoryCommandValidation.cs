using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public class UpdateCategoryCommandValidation : CategoryValidation<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidation()
        {
            ValidateName();
        }
    }
}

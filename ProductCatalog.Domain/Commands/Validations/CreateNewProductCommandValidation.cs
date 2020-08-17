using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public class CreateNewProductCommandValidation : ProductValidation<CreateNewProductCommand>
    {
        public CreateNewProductCommandValidation()
        {
            ValidateName();
            ValidateDescription();
            ValidateExternalId();
            ValidateImageUrl();
            ValidateUrl();
            ValidateDetailSpecs();
        }
    }
}

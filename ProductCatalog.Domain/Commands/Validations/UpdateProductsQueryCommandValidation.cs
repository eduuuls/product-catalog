using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public class UpdateProductsQueryCommandValidation : ProductValidation<UpdateProductsQueryCommand>
    {
        public UpdateProductsQueryCommandValidation()
        {
            ValidateId();
            ValidateName();
            ValidateImageUrl();
            ValidateDetailSpecs();
        }
    }
}

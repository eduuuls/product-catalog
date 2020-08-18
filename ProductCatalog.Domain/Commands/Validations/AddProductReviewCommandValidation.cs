using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands.Validations
{
    public class AddProductReviewCommandValidation : ProductReviewValidation<ProductReviewCommand>
    {
        public AddProductReviewCommandValidation()
        {
            ValidateExternalId();
            ValidateProductId();
            ValidateText();
            ValidateTitle();
        }
    }
}

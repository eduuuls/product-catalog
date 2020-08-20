using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Extensions
{
    public static class ProductExtensions
    {
        public static int GetRelevancePoints(this ProductDetail productDetail)
        {
            int relevancePoints = 0;

            if (!string.IsNullOrEmpty(productDetail.Model))
                relevancePoints += 6;

            if (!string.IsNullOrEmpty(productDetail.ReferenceModel))
                relevancePoints += 5;

            if (!string.IsNullOrEmpty(productDetail.BarCode))
                relevancePoints += 4;

            if (!string.IsNullOrEmpty(productDetail.Brand))
                relevancePoints += 3;

            if (!string.IsNullOrEmpty(productDetail.Manufacturer))
                relevancePoints += 2;

            if (!string.IsNullOrEmpty(productDetail.Supplier))
                relevancePoints += 1;

            return relevancePoints;
        }

    }
}

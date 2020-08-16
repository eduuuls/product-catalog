using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Application.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public ProductDetailViewModel Detail { get; set; }
        public List<ProductReviewViewModel> Reviews { get; set; }
    }
}

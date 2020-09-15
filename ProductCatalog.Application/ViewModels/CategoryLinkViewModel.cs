using System;

namespace ProductCatalog.Application.ViewModels
{
    public class CategoryLinkViewModel
    {
        public Guid CategoryId { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfProducts { get; set; }
    }
}

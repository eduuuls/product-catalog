using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Application.ViewModels
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfProducts { get; set; }
        public DataProvider DataProvider { get; set; }
        public IEnumerable<CategoryLinkViewModel> Links { get; set; }
    }
}

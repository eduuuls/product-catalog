using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Application.ViewModels
{
    public class ProductReviewViewModel
    {
        public string Reviewer { get; set; }
        public DateTime? Date { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public short? Starts { get; set; }
        public string Result { get; set; }
        public bool? IsRecommended { get; set; }
    }
}

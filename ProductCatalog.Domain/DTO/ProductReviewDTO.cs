using ProductCatalog.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Entities
{
    public class ProductReviewDTO
    {
        public string Reviewer { get; set; }
        public DateTime? Date { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public short? Stars { get; set; }
        public string Result { get; set; }
        public bool? IsRecommended { get; set; }
    }
}

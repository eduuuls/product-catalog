using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Application.ViewModels
{
    public class ProductDetailViewModel
    {
        public string Code { get; set; }
        public string BarCode { get; set; }
        public string Brand { get; set; }
        public string Manufacturer { get; set; }
        public string Supplier { get; set; }
        public string Model { get; set; }
        public string ReferenceModel { get; set; }
        public double BestPrice { get; set; }
        public string OtherSpecs { get; set; }

    }
}

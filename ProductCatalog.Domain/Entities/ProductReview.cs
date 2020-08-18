using ProductCatalog.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ProductCatalog.Domain.Entities
{
    public class ProductReview: Entity
    {
        public ProductReview()
        {

        }
        public ProductReview(Guid id, Guid productId, string externalId, string reviewer, DateTime? date,
                                    string title, string text, short? stars, string result, bool? isRecommended)
        {
            Id = id;
            ProductId = productId;
            ExternalId = externalId;
            Reviewer = reviewer;
            Date = date;
            Title = title;
            Text = text;
            Stars = stars;
            Result = result;
            IsRecommended = isRecommended;
        }

        public Guid ProductId { get; set; }
        public string ExternalId { get; set; }
        public string Reviewer { get; set; }
        public DateTime? Date { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public short? Stars { get; set; }
        public string Result { get; set; }
        public bool? IsRecommended { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
    }
}

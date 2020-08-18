using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.DTO
{
    public class B2WReviewDTO
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string SubmissionTime { get; set; }
        public string UserNickname { get; set; }
        public bool? IsRecommended { get; set; }
        public string Title { get; set; }
        public string ReviewText { get; set; }
        public short? Rating { get; set; }
    }
}

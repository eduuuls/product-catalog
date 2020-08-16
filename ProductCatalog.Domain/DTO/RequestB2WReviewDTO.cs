using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.DTO
{
    public class RequestB2WReviewDTO
    {
        public string Filter { get; set; }
        public string Sort { get; set; }
        public string Limit { get; set; }
        public string Offset { get; set; }

        //?&offset=0&limit=50&sort=SubmissionTime:asc&filter=ProductId:@id
    }
}

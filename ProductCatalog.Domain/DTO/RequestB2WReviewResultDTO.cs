using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.DTO
{
    public class RequestB2WReviewResultDTO
    {
        public int TotalResults { get; set; }
        public List<B2WReviewDTO> Results { get; set; }

        public bool Error { get; set; }
    }
}

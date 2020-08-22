using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Configuration
{
    public class FirebaseConfiguration
    {
        public string Apikey { get; set; }
        public string AuthEmail { get; set; }
        public string AuthPassword { get; set; }
        public string StorageBucketUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Application.Configuration
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString { get; set; }
        public List<Topic> Topics { get; set; }
    }
}

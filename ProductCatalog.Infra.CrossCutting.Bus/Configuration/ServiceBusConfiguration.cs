using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Infra.CrossCutting.Bus
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString { get; set; }
        public List<Topic> Topics { get; set; }
    }
}

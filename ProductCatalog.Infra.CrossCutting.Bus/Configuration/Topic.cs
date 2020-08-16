using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Infra.CrossCutting.Bus
{
    public class Topic
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public List<TopicSubscription> Subscriptions { get; set; }
    }
}

using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;

namespace ProductCatalog.Infra.CrossCutting.Resilience
{
    public static class RetryPolices
    {
        public static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(message => message.StatusCode != HttpStatusCode.OK)
                            .WaitAndRetryAsync(new[]
                              {
                                TimeSpan.FromSeconds(5),
                                TimeSpan.FromSeconds(15),
                                TimeSpan.FromSeconds(30),
                                TimeSpan.FromSeconds(45),
                                TimeSpan.FromSeconds(60)
                              });
        }
    }
}

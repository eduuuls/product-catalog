using System.Net.Http;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Configuration;

namespace ProductCatalog.Infra.Data.ExternalServices
{
    public class SubmarinoExternalService : Base.B2WExternalService, ISubmarinoExternalService
    {
        public SubmarinoExternalService(ILogger<SubmarinoExternalService> logger,
                                            IHttpClientFactory httpClientFactory,
                                                IOptions<ExternalServicesConfiguraiton> externalServicesConfiguraiton,
                                                    IOptions<FirebaseConfiguration> firebaseConfiguration,
                                                        ChromeOptions chromeOptions) 
            : base(httpClientFactory,
                    logger,
                        chromeOptions,
                            externalServicesConfiguraiton,
                                firebaseConfiguration,
                                    DataProvider.Submarino)
        {
        }
    }
}

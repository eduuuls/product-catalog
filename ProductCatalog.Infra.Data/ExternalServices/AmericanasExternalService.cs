using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Options;
using ProductCatalog.Infra.Data.Configuration;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Infra.Data.ExternalServices
{
    public class AmericanasExternalService : Base.B2WExternalService, IAmericanasExternalService
    {        
        public AmericanasExternalService(ILogger<AmericanasExternalService> logger, 
                                            IHttpClientFactory httpClientFactory,
                                                IOptions<ExternalServicesConfiguraiton> externalServicesConfiguraiton,
                                                    IOptions<FirebaseConfiguration> firebaseConfiguration,
                                                        ChromeOptions chromeOptions) 
            : base(httpClientFactory,
                    logger,
                        chromeOptions,
                            externalServicesConfiguraiton, 
                                firebaseConfiguration, 
                                    DataProvider.Americanas)
        {
            
        }

    }
}

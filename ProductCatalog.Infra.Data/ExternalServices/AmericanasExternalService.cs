﻿using System.Net.Http;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Options;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Configuration;

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

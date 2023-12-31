﻿using System.Net.Http;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Configuration;

namespace ProductCatalog.Infra.Data.ExternalServices
{
    public class ShoptimeExternalService : Base.B2WExternalService, IShoptimeExternalService
    {
        public ShoptimeExternalService(ILogger<ShoptimeExternalService> logger,
                                            IHttpClientFactory httpClientFactory,
                                                IOptions<ExternalServicesConfiguraiton> externalServicesConfiguraiton,
                                                    IOptions<FirebaseConfiguration> firebaseConfiguration,
                                                        ChromeOptions chromeOptions) 
            : base(httpClientFactory,
                    logger,
                        chromeOptions,
                            externalServicesConfiguraiton,
                                firebaseConfiguration,
                                    DataProvider.Shoptime)
        {
        }

    }
}

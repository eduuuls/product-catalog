﻿using System.Net.Http;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Logging;
using ProductCatalog.Infra.Data.Configuration;
using Microsoft.Extensions.Options;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Infra.Data.ExternalServices
{
    public class SubmarinoExternalService : Base.B2WExternalService, ISubmarinoExternalService
    {
        public SubmarinoExternalService(ILogger<SubmarinoExternalService> logger,
                                            IHttpClientFactory httpClientFactory,
                                                IOptions<ExternalServicesConfiguraiton> externalServicesConfiguraiton,
                                                    ChromeOptions chromeOptions) 
            : base(httpClientFactory,
                    logger,
                        chromeOptions,
                            externalServicesConfiguraiton, DataProvider.Americanas)
        {
        }
    }
}

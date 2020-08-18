using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Chrome;
using ProductCatalog.Application.BackgroundServices;
using ProductCatalog.Application.Configuration;
using ProductCatalog.Application.AutoMapper;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using ProductCatalog.Infra.Data.Configuration;
using ProductCatalog.Infra.Data.ExternalServices;
using System.Collections.Generic;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.Jobs;
using ProductCatalog.Application.MessagePublishers;

namespace ProductCatalog.Infra.CrossCutting.IoC.Crawler
{
    public static class Bootstrapper
    {
        public static void RegisterAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DomainToViewModelMapping), typeof(ViewModelToDomainMapping));
        }
        public static void RegisterExternalServices(this IServiceCollection services)
        {
            services.AddTransient<ISubmarinoExternalService, SubmarinoExternalService>();
            services.AddTransient<IAmericanasExternalService, AmericanasExternalService>();
            services.AddTransient<IShoptimeExternalService, ShoptimeExternalService>();
            services.AddHttpClient<IBuscapeExternalService, BuscapeExternalService>();
            services.AddHttpClient<IMercadoLivreExternalService, MercadoLivreExternalService>();

            services.AddHttpClient("B2WClient");
            //.ConfigurePrimaryHttpMessageHandler(h =>
            //{
            //    return new HttpClientHandler
            //    {
            //        Proxy = new WebProxy
            //        {
            //            Address = new Uri("http://proxy-server.scraperapi.com:8001"),
            //            Credentials = new NetworkCredential(userName: $"scraperapi.render=true.session_number={RandomNumberGenerator.GetInt32(1, 500)}",
            //                                                    password: "5467bfa9b99839c256739a6f6ef409ad")
            //        },
            //        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; },
            //    };
            //});

            var chromeOpts = new ChromeOptions();

            chromeOpts.AddArguments(new List<string>()
            {
                "user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36 Edg/84.0.522.40",
                "ignore-certificate-errors",
                "--disable-dev-shm-usage",
                //"--headless",
                //"--disable-gpu",
                "--dns-prefetch-disable",
                "--no-proxy-server",
                "--log-level=3",
                "--silent"
            });

            services.AddSingleton(chromeOpts);
        }
        public static void RegisterConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ExternalServicesConfiguraiton>(opt => configuration.GetSection("ExternalServicesConfiguraiton").Bind(opt));
            services.Configure<FirebaseConfiguration>(opt => configuration.GetSection("FirebaseConfiguration").Bind(opt));
            services.Configure<ServiceBusConfiguration>(opt => configuration.GetSection("ServiceBusConfiguration").Bind(opt));
        }
        public static void RegisterJobs(this IServiceCollection services)
        {
            services.AddScoped<IProductCategoryJob, CategoryJob>();
            services.AddScoped<IProductJob, ProductJob>();
            services.AddScoped<IProductReviewsJob, ProductReviewsJob>();
        }
        public static void RegisterMessagePublishers(this IServiceCollection services)
        {
            services.AddScoped<IMessagePublisher<CategoryJob>, MessagePublisher<CategoryJob>>();
            services.AddScoped<IMessagePublisher<ProductJob>, MessagePublisher<ProductJob>>();
            services.AddScoped<IMessagePublisher<ProductReviewsJob>, MessagePublisher<ProductReviewsJob>>();
        }
    }
}

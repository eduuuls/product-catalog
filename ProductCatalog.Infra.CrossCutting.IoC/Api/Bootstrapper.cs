using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.Chrome;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.Services;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Domain.Interfaces.UoW;
using ProductCatalog.Infra.CrossCutting.Bus;
using ProductCatalog.Infra.Data.Configuration;
using ProductCatalog.Infra.Data.ExternalServices;
using ProductCatalog.Infra.Data.Persistance;
using ProductCatalog.Infra.Data.Persistance.Repositories;
using ProductCatalog.Infra.Data.Persistance.UoW;
using System.Collections.Generic;

namespace ProductCatalog.Infra.CrossCutting.IoC.Api
{
    public static class Bootstrapper
    {
        public static void RegisterConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ExternalServicesConfiguraiton>(opt => configuration.GetSection("ExternalServicesConfiguraiton").Bind(opt));
            services.Configure<ServiceBusConfiguration>(opt => configuration.GetSection("ServiceBusConfiguration").Bind(opt));
        }
        public static void RegisterExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISubmarinoExternalService, SubmarinoExternalService>();
            services.AddScoped<IAmericanasExternalService, AmericanasExternalService>();
            services.AddScoped<IShoptimeExternalService, ShoptimeExternalService>();
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
                "--silent-launch",
                "no-sandbox",
                "--disable-gpu",
                "--headless",
                "--disable-extensions"
            });

            services.AddSingleton(chromeOpts);
        }
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
        }
        public static void RegisterPersistanceConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("ConnectionStrings:productCatalog").Value;

            services.AddDbContext<ProductsCatalogDbContext>(options => options.UseSqlServer(connectionString));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
        public static void RegisterCommands(this IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<UpdateCategoryCommand, ValidationResult>, CategoryCommandHandler>();
            services.AddScoped<IRequestHandler<CreateNewCategoryCommand, ValidationResult>, CategoryCommandHandler>();
            services.AddScoped<IRequestHandler<CreateNewCategoriesCommand, ValidationResult>, CategoriesCommandHandler>();
            services.AddScoped<IRequestHandler<AddProductReviewsCommand, ValidationResult>, ProductReviewsCommandHandler>();
            services.AddScoped<IRequestHandler<CreateNewProductsCommand, ValidationResult>, ProductsCommandHandler>();
        }
        public static void RegisterEvents(this IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<CategoryCreatedEvent>, CategoryEventHandler>();
        }
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICategoriesRepository, CategoriesRepository>();
            services.AddScoped<IProductsRepository, ProductsRepository>();
            services.AddScoped<IProductsReviewsRepository, ProductsReviewsRepository>();
            services.AddScoped<IProductsDetailRepository, ProductsDetailRepository>();
        }
        public static IApplicationBuilder RunMigrations(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using var context = serviceScope.ServiceProvider.GetService<ProductsCatalogDbContext>();
                context.Database.Migrate();
            }
            return app;
        }
    }
}

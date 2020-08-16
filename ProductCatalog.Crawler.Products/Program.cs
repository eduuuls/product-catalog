using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Serilog;
using Microsoft.Extensions.Hosting;
using ProductCatalog.Infra.CrossCutting.IoC.Crawler;
using ProductCatalog.Application.BackgroundServices;

namespace ProductCatalog.Crawler.Products
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                    {
                        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                        configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    })
                   .ConfigureServices((hostContext, services) =>
                   {
                       var logger = new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration)
                                                                    .WriteTo.Console()
                                                                        .Enrich.WithThreadId()
                                                                            .CreateLogger();
     
                       services.AddLogging(builder =>
                       {
                           builder.AddSerilog(logger: logger, dispose: true);
                       });

                       services.RegisterMessagePublishers();
                       services.RegisterJobs();
                       services.RegisterExternalServices();
                       services.RegisterConfigurations(hostContext.Configuration);
                       services.RegisterAutoMapper();

                       services.AddHostedService<ImportProductsService>();
                   });
    }
}

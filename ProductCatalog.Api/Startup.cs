using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ProductCatalog.Application.AutoMapper;
using ProductCatalog.Infra.CrossCutting.IoC.Api;
using MediatR;
using ProductCatalog.Application.BackgroundServices;
using Serilog;
using ProductCatalog.Infra.CrossCutting.Bus;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.MessagePublishers;

namespace ProductCatalogApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.MaxDepth = 64;
            });
            
            services.AddScoped<IMediatorHandler, MessageBus>();

            services.RegisterConfigurations(Configuration);
            services.RegisterExternalServices();
            services.RegisterServices();
            services.RegisterRepositories();
            services.RegisterCommands();
            services.RegisterEvents();
            services.RegisterRepositories();
            services.RegisterPersistanceConfigurations(Configuration);
            services.RegisterQueries();

            services.AddAutoMapperConfiguration();
            services.AddMediatR(typeof(Startup));
            services.AddHostedService<CreateCategoriesConsumer>();
            services.AddHostedService<CreateProductsConsumer>();
            services.AddHostedService<AddProductReviewsConsumer>();
            services.AddHostedService<ProductDataChangedEventConsumer>();

            var logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();

            services.AddLogging(builder =>
            {
                builder.AddSerilog(logger: logger, dispose: true);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product Catalog API", Version = "v1" });
            });
            services.AddCors();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(c =>
            {
                c.AllowAnyOrigin();
                c.AllowAnyHeader();
                c.AllowAnyMethod();
            });

            app.UseRouting();
            
            app.UseSwagger();

            app.UseAuthorization();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Catalog API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.RunMigrations();

        }
    }
}

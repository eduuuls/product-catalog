using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalog.Application.Jobs
{
    public class CategoryJob: Base.BaseJob, IProductCategoryJob
    {
        private readonly IShoptimeExternalService _shoptimeExternalService;
        private readonly IAmericanasExternalService _americanasExternalService;
        private readonly ISubmarinoExternalService _submarinoExternalService;
        private readonly IMessagePublisher<CategoryJob> _messagePublisher;

        public CategoryJob(ILogger<CategoryJob> logger, IMapper mapper, IMessagePublisher<CategoryJob> messagePublisher, 
                                        IShoptimeExternalService shoptimeExternalService,  IAmericanasExternalService americanasExternalService, 
                                            ISubmarinoExternalService submarinoExternalService)
            : base(logger, mapper)
        {
            _messagePublisher = messagePublisher;
            _americanasExternalService = americanasExternalService;
            _shoptimeExternalService = shoptimeExternalService;
            _submarinoExternalService = submarinoExternalService;
        }

        public async Task ImportCategories(DataProvider dataProvider)
        {
            LogInfo($"[ProductCategoryJobs] Starting Job...");

            switch (dataProvider)
            {
                case DataProvider.Americanas:
                    await ImportFromAmericanas();
                    break;
                case DataProvider.Submarino:
                    await ImportFromAmericanas();
                    break;
                case DataProvider.Shoptime:
                    await ImportFromAmericanas();
                    break;
            }

            LogInfo($"[ProductCategoryJobs] Finishing Job...");
        }

        private async Task ImportFromAmericanas()
        {
            LogInfo($"[ProductCategoryJobs] ImportFromAmericanas - Getting categories...");
            var americanasCategories = await _americanasExternalService.GetProductCategories();
            List<Task> tasks = new List<Task>();
            
            var commands = americanasCategories.Select(category =>
            {
                return new CreateNewCategoryCommand(category.Name, category.SubType, category.Description, category.Url, category.ImageUrl, 
                                                        category.IsActive, category.NumberOfProducts, category.DataProvider);
            }).ToList();

            if (commands.Any())
            {
                try
                {
                    LogInfo($"[ProductCategoryJobs] Queueing categories: {commands.Count}");

                    var queueTask = _messagePublisher.Publish(new CreateNewCategoriesCommand(commands));

                    queueTask.Wait();

                    LogInfo($"[ProductCategoryJobs] {commands.Count} categories queued!");
                }
                catch (Exception ex)
                {
                    LogError($"[Error] {ex.Message}");
                }
            }

            LogInfo($"[ProductCategoryJobs] ImportFromAmericanas - Categories created...");
        }

        //private async Task ImportFromSubmarino()
        //{
        //    List<Task> tasks = new List<Task>();

        //    var submarinoCategories = await _submarinoExternalService.GetProductCategories();

        //    foreach (var category in submarinoCategories)
        //    {
        //        var task = Task.Run(async () =>
        //        {
        //            await SendCreationCommand(category);
        //        });

        //        tasks.Add(task);
        //    };
        //}

        //private async Task ImportFromShoptime()
        //{
        //    List<Task> tasks = new List<Task>();

        //    var shoptimeCategories = await _shoptimeExternalService.GetProductCategories();

        //    foreach (var category in shoptimeCategories)
        //    {
        //        var task = Task.Run(async () =>
        //        {
        //            await SendCreationCommand(category);
        //        });

        //        tasks.Add(task);
        //    };
        //}
    }
}

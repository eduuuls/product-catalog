using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.DTO;
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
                    LogInfo($"[ProductCategoryJobs] ImportFromAmericanas - Getting categories...");
                    QueueCategories(await _americanasExternalService.GetProductCategories());
                    LogInfo($"[ProductCategoryJobs] ImportFromAmericanas - Categories imported!");
                    break;
                case DataProvider.Submarino:
                    LogInfo($"[ProductCategoryJobs] ImportFromSubmarino - Getting categories...");
                    QueueCategories(await _submarinoExternalService.GetProductCategories());
                    LogInfo($"[ProductCategoryJobs] ImportFromSubmarino - Categories imported!");
                    break;
                case DataProvider.Shoptime:
                    LogInfo($"[ProductCategoryJobs] ImportFromShoptime - Getting categories...");
                    QueueCategories(await _shoptimeExternalService.GetProductCategories());
                    LogInfo($"[ProductCategoryJobs] ImportFromShoptime - Categories imported!");
                    break;
            }

            LogInfo($"[ProductCategoryJobs] Job finished...");
        }

        private void QueueCategories(List<CategoryDTO> categories)
        {
            var commands = categories.Select(category =>
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
        }
    }
}

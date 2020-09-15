using AutoMapper;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.DTO;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using ScrapySharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalog.Application.Jobs
{
    public class CategoryJob : Base.BaseJob, IProductCategoryJob
    {
        private readonly IShoptimeExternalService _shoptimeExternalService;
        private readonly IAmericanasExternalService _americanasExternalService;
        private readonly ISubmarinoExternalService _submarinoExternalService;
        private readonly IMessagePublisher<CategoryJob> _messagePublisher;

        public CategoryJob(ILogger<CategoryJob> logger, IMapper mapper, IMessagePublisher<CategoryJob> messagePublisher,
                                        IShoptimeExternalService shoptimeExternalService, IAmericanasExternalService americanasExternalService,
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
            LogInfo($"[CategoryJobs] Starting Job...");
            LogInfo($"[CategoryJobs] {dataProvider} - Getting categories...");
            var opts = new ParallelOptions() { MaxDegreeOfParallelism = 5 };

            switch (dataProvider)
            {
                case DataProvider.Americanas:

                    var americanasCategories = await _americanasExternalService.GetCategoriesData();

                    Parallel.ForEach(americanasCategories, opts, categoryDTO =>
                    {
                        categoryDTO.Links = _americanasExternalService.GetCategoryAdditionalLinks(categoryDTO);

                        if (categoryDTO.Links.Any())
                        {
                            categoryDTO.NumberOfProducts = categoryDTO.Links.Sum(l => l.NumberOfProducts);
                            QueueCategoriesAsync(categoryDTO).Wait();
                        }
                    });

                    LogInfo($"[CategoryJobs] {dataProvider} - Number of imported categories: {americanasCategories.Count()}");

                    break;

                case DataProvider.Submarino:

                    var submarinoCategories = await _submarinoExternalService.GetCategoriesData();

                    Parallel.ForEach(submarinoCategories, opts, categoryDTO =>
                    {
                        categoryDTO.Links = _submarinoExternalService.GetCategoryAdditionalLinks(categoryDTO);

                        if (categoryDTO.Links.Any())
                        {
                            categoryDTO.NumberOfProducts = categoryDTO.Links.Sum(l => l.NumberOfProducts);
                            QueueCategoriesAsync(categoryDTO).Wait();
                        }
                    });

                    LogInfo($"[CategoryJobs] {dataProvider} - Number of imported categories: {submarinoCategories.Count()}");

                    break;

                case DataProvider.Shoptime:

                    var shoptimeCategories = await _shoptimeExternalService.GetCategoriesData();

                    Parallel.ForEach(shoptimeCategories, opts, categoryDTO =>
                    {
                        categoryDTO.Links = _shoptimeExternalService.GetCategoryAdditionalLinks(categoryDTO);

                        if (categoryDTO.Links.Any())
                        {
                            categoryDTO.NumberOfProducts = categoryDTO.Links.Sum(l => l.NumberOfProducts);
                            QueueCategoriesAsync(categoryDTO).Wait();
                        }
                    });

                    LogInfo($"[CategoryJobs] {dataProvider} - Number of imported categories: {shoptimeCategories.Count()}");
                    break;
            }


            LogInfo($"[CategoryJobs] Job finished...");
        }

        private async Task QueueCategoriesAsync(CategoryDTO category)
        {
            var links = _mapper.Map<List<CategoryLink>>(category.Links);

            var command = new CreateNewCategoryCommand(category.Name, category.Description, category.ImageUrl, category.IsActive,
                                                    category.NumberOfProducts, category.DataProvider, links);

            LogInfo($"[ProductCategoryJobs] Queueing category {category.Name} with a total of {category.NumberOfProducts} products.");

            try
            {
                await _messagePublisher.Publish(command);

            }
            catch (Exception ex)
            {
                LogError($"[Error] {ex.Message}");
            }

            LogInfo($"[ProductCategoryJobs] Category {category.Name} queued!");
        }
    }
}

using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.DTO;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.ViewModels;

namespace ProductCatalog.Application.Jobs
{
    public class ProductJob : Base.BaseJob, IProductJob
    {
        private const int CATEGORY_PAGE_SIZE = 24;
        private const int MAX_PAGES = 20;

        private readonly IAmericanasExternalService _americanasExternalService;
        private readonly IMessagePublisher<ProductJob> _messagePublisher;
        public ProductJob(ILogger<ProductJob> logger, IMapper mapper, IMessagePublisher<ProductJob> messagePublisher,
                                    IAmericanasExternalService americanasExternalService)
            : base(logger, mapper)
        {
            _americanasExternalService = americanasExternalService;
            _messagePublisher = messagePublisher;
        }

        public void ImportProductsJob(CategoryViewModel categoryViewModel)
        {
            LogInfo($"[ProductJobs] Starting Job...");

            switch (categoryViewModel.DataProvider)
            {
                case DataProvider.Americanas:
                    ImportFromAmericanas(categoryViewModel);
                    break;
                case DataProvider.Submarino:
                    ImportFromAmericanas(categoryViewModel);
                    break;
                case DataProvider.Shoptime:
                    ImportFromAmericanas(categoryViewModel);
                    break;
                case DataProvider.Buscape:
                    break;
            }

            LogInfo($"[ProductJobs] Finishing Job...");
        }

        private void ImportFromAmericanas(CategoryViewModel categoryViewModel)
        {
            List<ProductDTO> products = new List<ProductDTO>();
            List<Task> tasks = new List<Task>();

            int numberOfPages = categoryViewModel.NumberOfProducts / CATEGORY_PAGE_SIZE;

            if (numberOfPages > MAX_PAGES)
                numberOfPages = MAX_PAGES;

            LogInfo($"[ProductJobs] ImportFromAmericanas - Getting products...");

            Parallel.For(1, numberOfPages, (index) =>
            {
                var url = $"{categoryViewModel.Url}/pagina-{index}";

                LogInfo($"[ProductJobs] ImportFromAmericanas - Getting products from category:{categoryViewModel.Name}...");

                var americanasProducts = _americanasExternalService.GetProductsByCategory(categoryViewModel.Id, url);

                products.AddRange(americanasProducts);
            });

            var commands = products.Select(product =>
            {
                var reviews = _mapper.Map<List<ProductReview>>(product.Reviews);

                return new CreateNewProductCommand(product.CategoryId, product.ExternalId, product.Name, product.Description,
                                                    product.BarCode, product.Code, product.Brand, product.Manufacturer, product.Model,
                                                        product.ReferenceModel, product.Supplier, product.OtherSpecs, product.Url,
                                                            product.ImageUrl, product.DataProvider, reviews);
            }).ToList();

            var task = Task.Run(async () =>
            {
                try
                {
                    LogInfo($"[ProductJobs] Queueing products: {commands.Count}");

                    await _messagePublisher.Publish(new CreateNewProductsCommand(commands));

                    LogInfo($"[ProductJobs] {commands.Count} products queued!");
                }
                catch (Exception ex)
                {
                    LogError($"[Error] {ex.Message}");
                }
            });

            tasks.Add(task);

            Task.WaitAll(tasks.ToArray());

            LogInfo($"[ProductJobs] ImportFromAmericanas - products created...");
        }
    }
}

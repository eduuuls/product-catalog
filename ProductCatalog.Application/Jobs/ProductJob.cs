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

        private readonly IShoptimeExternalService _shoptimeExternalService;
        private readonly IAmericanasExternalService _americanasExternalService;
        private readonly ISubmarinoExternalService _submarinoExternalService;
        private readonly IMessagePublisher<ProductJob> _messagePublisher;
        public ProductJob(ILogger<ProductJob> logger, IMapper mapper, IMessagePublisher<ProductJob> messagePublisher,
                                    IShoptimeExternalService shoptimeExternalService, 
                                        IAmericanasExternalService americanasExternalService,
                                            ISubmarinoExternalService submarinoExternalService)
            : base(logger, mapper)
        {
            _americanasExternalService = americanasExternalService;
            _shoptimeExternalService = shoptimeExternalService;
            _submarinoExternalService = submarinoExternalService;
            _messagePublisher = messagePublisher;
        }

        public void ImportProducts(CategoryViewModel categoryViewModel)
        {
            LogInfo($"[ProductJobs] Starting Job...");

            List<ProductDTO> products = new List<ProductDTO>();

            int numberOfPages = categoryViewModel.NumberOfProducts / CATEGORY_PAGE_SIZE;

            if (numberOfPages <= 0)
                numberOfPages = 1;
            if (numberOfPages > MAX_PAGES)
                numberOfPages = MAX_PAGES;

            LogInfo($"[ProductJobs] ImportProducts - Getting products...");

            Parallel.For(0, numberOfPages, (index) =>
            {
                var url = $"{categoryViewModel.Url}/pagina-{index + 1}";

                LogInfo($"[ProductJobs] ImportProducts - Getting products from category:{categoryViewModel.Name}...");

                switch (categoryViewModel.DataProvider)
                {
                    case DataProvider.Americanas:
                        products.AddRange(_americanasExternalService.GetProductsByCategory(categoryViewModel.Id, url));
                        break;
                    case DataProvider.Submarino:
                        products.AddRange(_submarinoExternalService.GetProductsByCategory(categoryViewModel.Id, url));
                        break;
                    case DataProvider.Shoptime:
                        products.AddRange(_shoptimeExternalService.GetProductsByCategory(categoryViewModel.Id, url));
                        break;
                    case DataProvider.Buscape:
                        break;
                }
            });

            QueueProducts(products);

            LogInfo($"[ProductJobs] ImportProducts - Products imported!");
            LogInfo($"[ProductJobs] Job finished...");
        }

        private void QueueProducts(List<ProductDTO> products)
        {
            var commands = products.Select(product =>
            {
                var reviews = _mapper.Map<List<ProductReview>>(product.Reviews);

                return new CreateNewProductCommand(product.CategoryId, product.ExternalId, product.Name, product.Description,
                                                    product.BarCode, product.Code, product.Brand, product.Manufacturer, product.Model,
                                                        product.ReferenceModel, product.Supplier, product.OtherSpecs, product.Url,
                                                            product.ImageUrl, product.DataProvider, reviews);
            }).ToList();

            if (commands.Any())
            {
                try
                {
                    LogInfo($"[ProductJobs] Queueing products: {commands.Count}");

                    var queueTask = _messagePublisher.Publish(new CreateNewProductsCommand(commands));

                    queueTask.Wait();

                    LogInfo($"[ProductJobs] {commands.Count} products queued!");
                }
                catch (Exception ex)
                {
                    LogError($"[Error] {ex.Message}");
                }
            }
        }
    }
}

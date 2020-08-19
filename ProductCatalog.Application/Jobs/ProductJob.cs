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
using System.Runtime.CompilerServices;

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

        public async Task ImportProducts(CategoryViewModel categoryViewModel)
        {
            LogInfo($"[ProductJobs] Starting Job...");
            var opts = new ParallelOptions() { MaxDegreeOfParallelism = 20 };

            int numberOfPages = categoryViewModel.NumberOfProducts / CATEGORY_PAGE_SIZE;

            if (numberOfPages <= 0)
                numberOfPages = 1;
            if (numberOfPages > MAX_PAGES)
                numberOfPages = MAX_PAGES;

            LogInfo($"[ProductJobs] ImportProducts - Getting products...");

            for (int index = 0; index <= numberOfPages; index++)
            {
                var url = $"{categoryViewModel.Url}/pagina-{index + 1}";

                LogInfo($"[ProductJobs] ImportProducts - Getting products from category:{categoryViewModel.Name}...");

                switch (categoryViewModel.DataProvider)
                {
                    case DataProvider.Americanas:

                        var americanasProducts = await _americanasExternalService.GetProductsData(categoryViewModel.Id, url);

                        Parallel.ForEach(americanasProducts, opts, async productDTO =>
                        {
                            var product = await _americanasExternalService.GetProductAdditionalData(categoryViewModel.Id, productDTO);

                            await QueueProducts(product);
                        });

                        LogInfo($"[ProductJobs] Americanas - Number of obtained products:{americanasProducts.Count()}");

                        break;
                    case DataProvider.Submarino:

                        var submarinoProducts = await _submarinoExternalService.GetProductsData(categoryViewModel.Id, url);

                        Parallel.ForEach(submarinoProducts, opts, async productDTO =>
                        {
                            var product = await _submarinoExternalService.GetProductAdditionalData(categoryViewModel.Id, productDTO);

                            await QueueProducts(product);
                        });

                        LogInfo($"[ProductJobs] Submarino - Number of obtained products:{submarinoProducts.Count()}");
                        break;
                    case DataProvider.Shoptime:
                        var shoptimeProducts = await _shoptimeExternalService.GetProductsData(categoryViewModel.Id, url);

                        Parallel.ForEach(shoptimeProducts, opts, async productDTO =>
                        {
                            var product = await _shoptimeExternalService.GetProductAdditionalData(categoryViewModel.Id, productDTO);

                            await QueueProducts(product);
                        });

                        LogInfo($"[ProductJobs] Shoptime - Number of obtained products:{shoptimeProducts.Count()}");
                        break;
                    case DataProvider.Buscape:
                        break;
                }
            }

            LogInfo($"[ProductJobs] ImportProducts - Products imported!");
            
            LogInfo($"[ProductJobs] Job finished...");
        }

        private async Task QueueProducts(ProductDTO product)
        {
            var reviews = _mapper.Map<List<ProductReview>>(product.Reviews);

            var command = new CreateNewProductCommand(product.CategoryId, product.ExternalId, product.Name, product.Description,
                                                 product.BarCode, product.Code, product.Brand, product.Manufacturer, product.Model,
                                                     product.ReferenceModel, product.Supplier, product.OtherSpecs, product.Url,
                                                         product.ImageUrl, product.DataProvider, reviews);
            
            try
            {
                LogInfo($"[QueueProducts] Queueing product: {command.Name}");

                await _messagePublisher.Publish(command);

                LogInfo($"[QueueProducts] {command.Name} queued!");
            }
            catch (Exception ex)
            {
                LogError($"[QueueProducts] Error - {ex.Message}");
            }
        }
    }
}

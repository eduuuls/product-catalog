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
    public class ProductReviewsJob : Base.BaseJob, IProductReviewsJob
    {
        private const int CATEGORY_PAGE_SIZE = 24;
        private const int MAX_PAGES = 20;
        private const int MAX_REVIEWS = 300;

        private readonly IAmericanasExternalService _americanasExternalService;
        private readonly IMessagePublisher<ProductReviewsJob> _messagePublisher;
        public ProductReviewsJob(ILogger<ProductReviewsJob> logger, IMapper mapper, IMessagePublisher<ProductReviewsJob> messagePublisher,
                                    IAmericanasExternalService americanasExternalService)
            : base(logger, mapper)
        {
            _americanasExternalService = americanasExternalService;
            _messagePublisher = messagePublisher;
        }

        public void ImportProductReviews(ProductViewModel productViewModel)
        {
            LogInfo($"[ProductReviewsJob] Starting Job...");

            switch (productViewModel.DataProvider)
            {
                case DataProvider.Americanas:
                    ImportFromAmericanas(productViewModel);
                    break;
                case DataProvider.Submarino:
                    ImportFromAmericanas(productViewModel);
                    break;
                case DataProvider.Shoptime:
                    ImportFromAmericanas(productViewModel);
                    break;
                case DataProvider.Buscape:
                    break;
            }

            LogInfo($"[ProductReviewsJob] Finishing Job...");
        }

        private void ImportFromAmericanas(ProductViewModel productViewModel)
        {
            LogInfo($"[ProductReviewsJob] ImportFromAmericanas - Getting reviews...");

            List<ProductReview> reviews = new List<ProductReview>();
            bool goNext = true;
            int offset = 0;

            //?&offset=0&limit=50&sort=SubmissionTime:asc&filter=ProductId:@id
            RequestB2WReviewDTO requestB2WReviewDTO = new RequestB2WReviewDTO();
            requestB2WReviewDTO.Filter = $"filter=ProductId:{productViewModel.ExternalId}";
            requestB2WReviewDTO.Limit = "limit=50";
            requestB2WReviewDTO.Sort = "sort=SubmissionTime:asc";

            while(goNext)
            {
                LogInfo($"[ProductReviewsJob] ImportFromAmericanas - Getting reviews for product:{productViewModel.Name}...");
                
                requestB2WReviewDTO.Offset = $"offset={offset}";

                var productReviewTask = _americanasExternalService.GetProductReviews(requestB2WReviewDTO);

                productReviewTask.Wait();

                if (productReviewTask.Result != null)
                {
                    reviews.AddRange(_mapper.Map<List<ProductReview>>(productReviewTask.Result));

                    goNext = productReviewTask.Result.Count == 50;
                }
                else
                    goNext = false;

                offset += 50;

                if(reviews.Count >= MAX_REVIEWS)
                    goNext = false;
            }

            var commands = reviews.Select(review =>
            {
                return new AddProductReviewCommand(productViewModel.Id, review.ExternalId, review.Reviewer, review.Date, review.Title,
                                                        review.Text, review.Stars, review.Result, review.IsRecommended);
            }).ToList();

            if (commands.Any())
            {
                try
                {
                    LogInfo($"[ProductCategoryJobs] Queueing product reviews: {commands.Count}");

                    var queueTask = _messagePublisher.Publish(new AddProductReviewsCommand(commands));

                    queueTask.Wait();

                    LogInfo($"[ProductReviewsJob] Product reviews queued!");
                }
                catch (Exception ex)
                {
                    LogError($"[Error] {ex.Message}");
                }
            }
        }
    }
}

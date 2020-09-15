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

        public async Task ImportProductReviews(ProductViewModel productViewModel)
        {
            LogInfo($"[ProductReviewsJob] Starting Job...");
            LogInfo($"[ProductReviewsJob] ImportFromAmericanas - Getting reviews...");

            List<ProductReview> reviews = new List<ProductReview>();
            bool goNext = true;
            int offset = 0;

            //?&offset=0&limit=50&sort=SubmissionTime:asc&filter=ProductId:@id
            RequestB2WReviewDTO requestB2WReviewDTO = new RequestB2WReviewDTO();
            requestB2WReviewDTO.Filter = $"filter=ProductId:{productViewModel.ExternalId}";
            requestB2WReviewDTO.Limit = "limit=50";
            requestB2WReviewDTO.Sort = "sort=SubmissionTime:asc";

            while (goNext)
            {
                LogInfo($"[ProductReviewsJob] ImportFromAmericanas - Getting reviews for product:{productViewModel.Name}...");

                requestB2WReviewDTO.Offset = $"offset={offset}";

                switch (productViewModel.DataProvider)
                {
                    case DataProvider.Americanas:

                        var americanasProductReviews = await _americanasExternalService.GetProductReviews(requestB2WReviewDTO);
                        
                        if (americanasProductReviews != null)
                        {
                            reviews.AddRange(_mapper.Map<List<ProductReview>>(americanasProductReviews));

                            goNext = americanasProductReviews.Count == 50;
                        }
                        else
                            goNext = false;

                        break;
                    case DataProvider.Submarino:

                        var submarinoProductReviews = await _americanasExternalService.GetProductReviews(requestB2WReviewDTO);

                        if (submarinoProductReviews != null)
                        {
                            reviews.AddRange(_mapper.Map<List<ProductReview>>(submarinoProductReviews));

                            goNext = submarinoProductReviews.Count == 50;
                        }
                        else
                            goNext = false;

                        break;
                    case DataProvider.Shoptime:

                        var shoptimeProductReviews = await _americanasExternalService.GetProductReviews(requestB2WReviewDTO);

                        if (shoptimeProductReviews != null)
                        {
                            reviews.AddRange(_mapper.Map<List<ProductReview>>(shoptimeProductReviews));

                            goNext = shoptimeProductReviews.Count == 50;
                        }
                        else
                            goNext = false;

                        break;
                    case DataProvider.Buscape:
                        break;
                }
                
                offset += 50;

                if (reviews.Count >= MAX_REVIEWS)
                    goNext = false;
            }

            await QueueReviews(productViewModel.Id, reviews);

            LogInfo($"[ProductReviewsJob] Finishing Job...");
        }

        private async Task QueueReviews(Guid id, List<ProductReview> reviews)
        {
            var commands = reviews.Select(review =>
            {
                return new AddProductReviewCommand(id, review.ExternalId, review.Reviewer, review.Date, review.Title,
                                                        review.Text, review.Stars, review.Result, review.IsRecommended);
            }).ToList();

            if (commands.Any())
            {
                try
                {
                    LogInfo($"[ProductCategoryJobs] Queueing product reviews: {commands.Count}");

                    await _messagePublisher.Publish(new AddProductReviewsCommand(commands));

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

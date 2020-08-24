using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Commands
{
    public class ProductReviewsCommandHandler : CommandHandler, IRequestHandler<AddProductReviewsCommand, ValidationResult>
    {
        private readonly IProductsReviewsRepository _productsReviewsRepository;

        public ProductReviewsCommandHandler(ILogger<ProductReviewsCommandHandler> logger, IProductsReviewsRepository productsReviewsRepository)
            : base(logger)
        {
            _productsReviewsRepository = productsReviewsRepository;
        }

        public async Task<ValidationResult> Handle(AddProductReviewsCommand message, CancellationToken cancellationToken)
        {
            List<ProductDataChangedEvent> registredEvents = new List<ProductDataChangedEvent>();

            LogInfo($"[Handle] Starting handling product reviews creation...");
            
            message.Commands.ForEach(c =>
            {
                LogInfo($"[Handle] Handling review: {c.ExternalId}");

                LogInfo($"[Handle] Validating review...");
                if (!c.IsValid())
                {
                    LogInfo($"[Handle] Review didn't pass validation process...");
                    return;
                }
                
                LogInfo($"[Handle] Review validation Ok...");

                var productReview = new ProductReview(Guid.NewGuid(), c.ProductId, c.ExternalId, c.Reviewer, c.Date, 
                                                    c.Title, c.Text, c.Stars, c.Result, c.IsRecommended);

                var task = _productsReviewsRepository.GetByKey(productReview.ProductId, productReview.ExternalId);

                task.Wait();

                var existingReview = task.Result;

                if (existingReview == null)
                {
                    LogInfo($"[Handle] Creating new product review for product: {c.ProductId}");

                    _productsReviewsRepository.Add(productReview);
                }
                else
                    LogInfo($"[Handle] Review already exists!");

                if (!registredEvents.Any(p => p.ProductId == productReview.ProductId))
                {
                    var productDataChangedEvent = new ProductDataChangedEvent(productReview.ProductId);
                    productReview.AddDomainEvent(productDataChangedEvent);
                    registredEvents.Add(productDataChangedEvent);
                }
            });

            LogInfo($"[Handle] Commiting process...");

            return await Commit(_productsReviewsRepository.UnitOfWork);
        }
    }
}

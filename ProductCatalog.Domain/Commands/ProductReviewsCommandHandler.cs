using FluentValidation.Results;
using MediatR;
using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Commands
{
    public class ProductReviewsCommandHandler : CommandHandler, IRequestHandler<AddProductReviewsCommand, ValidationResult>
    {
        private readonly IProductsReviewsRepository _productsReviewsRepository;

        public ProductReviewsCommandHandler(IProductsReviewsRepository productsReviewsRepository)
        {
            _productsReviewsRepository = productsReviewsRepository;
        }

        public async Task<ValidationResult> Handle(AddProductReviewsCommand message, CancellationToken cancellationToken)
        {
            message.Commands.ForEach(c =>
            {
                if (!c.IsValid()) return;

                var productReview = new ProductReview(Guid.NewGuid(), c.ProductId, c.ExternalId, c.Reviewer, c.Date, 
                                                    c.Title, c.Text, c.Stars, c.Result, c.IsRecommended);

                var task = _productsReviewsRepository.GetByKey(productReview.ProductId, productReview.ExternalId);

                task.Wait();

                var existingReview = task.Result;

                if (existingReview == null)
                    _productsReviewsRepository.Add(productReview);

            });

            return await Commit(_productsReviewsRepository.UnitOfWork);
        }
    }
}

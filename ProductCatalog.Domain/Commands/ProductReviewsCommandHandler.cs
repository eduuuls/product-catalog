using FluentValidation.Results;
using MediatR;
using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Commands
{
    public class ProductReviewsCommandHandler : CommandHandler, IRequestHandler<AddProductReviewsCommand, ValidationResult>
    {
        private readonly IProductsRepository _productsRepository;

        public ProductReviewsCommandHandler(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        public async Task<ValidationResult> Handle(AddProductReviewsCommand message, CancellationToken cancellationToken)
        {
            var product = await _productsRepository.GetById(message.ProductId);

            if (product != null)
            {
                product.Reviews = message.Reviews;

                _productsRepository.Update(product);
            }

            return await Commit(_productsRepository.UnitOfWork);
        }
    }
}

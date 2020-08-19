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
    public class ProductsCommandHandler : CommandHandler, 
                                            IRequestHandler<CreateNewProductsCommand, ValidationResult>,
                                            IRequestHandler<CreateNewProductCommand, ValidationResult>
    {
        private readonly IProductsRepository _productsRepository;

        public ProductsCommandHandler(IProductsRepository productsRepository)
        {
            _productsRepository = productsRepository;
        }

        public async Task<ValidationResult> Handle(CreateNewProductsCommand message, CancellationToken cancellationToken)
        {
            message.Commands.ForEach(c =>
            {
                if (!c.IsValid()) return;

                var product = new Product(Guid.NewGuid(), c.CategoryId, c.ExternalId, c.Name, c.Description, c.BarCode,
                                            c.Code, c.Brand, c.Manufacturer, c.Model, c.ReferenceModel, c.Supplier, c.OtherSpecs,
                                                c.Url, c.ImageUrl, c.DataProvider, c.Reviews);

                var task = _productsRepository.GetByKey(product.CategoryId, product.ExternalId, product.DataProvider);

                task.Wait();

                var existingProduct = task.Result;

                if (existingProduct != null)
                {
                    existingProduct.Description = product.Description;
                    existingProduct.Name = product.Name;
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.Url = product.Url;
                    existingProduct.Detail = product.Detail;

                    product.AddDomainEvent(new ProductUpdatedEvent(existingProduct.Id, existingProduct.CategoryId, existingProduct.ExternalId, existingProduct.Name,
                                                                    existingProduct.Description, existingProduct.Url, existingProduct.ImageUrl, existingProduct.DataProvider,
                                                                        existingProduct.Detail, existingProduct.Reviews));
                }
                else
                {
                    product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.CategoryId, product.ExternalId, product.Name,
                                                                    product.Description, product.Url, product.ImageUrl, product.DataProvider,
                                                                        product.Detail, product.Reviews));

                    _productsRepository.Add(product);
                }
            });

            return await Commit(_productsRepository.UnitOfWork);
        }

        public async Task<ValidationResult> Handle(CreateNewProductCommand message, CancellationToken cancellationToken)
        {

            if (!message.IsValid()) return message.ValidationResult;

            var product = new Product(Guid.NewGuid(), message.CategoryId, message.ExternalId, message.Name, message.Description, message.BarCode,
                                        message.Code, message.Brand, message.Manufacturer, message.Model, message.ReferenceModel, message.Supplier, 
                                            message.OtherSpecs, message.Url, message.ImageUrl, message.DataProvider, message.Reviews);

            var task = _productsRepository.GetByKey(product.CategoryId, product.ExternalId, product.DataProvider);

            task.Wait();

            var existingProduct = task.Result;

            if (existingProduct != null)
            {
                existingProduct.Description = product.Description;
                existingProduct.Name = product.Name;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Url = product.Url;
                existingProduct.Detail = product.Detail;

                product.AddDomainEvent(new ProductUpdatedEvent(existingProduct.Id, existingProduct.CategoryId, existingProduct.ExternalId, existingProduct.Name,
                                                                existingProduct.Description, existingProduct.Url, existingProduct.ImageUrl, existingProduct.DataProvider,
                                                                    existingProduct.Detail, existingProduct.Reviews));
            }
            else
            {
                product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.CategoryId, product.ExternalId, product.Name,
                                                                product.Description, product.Url, product.ImageUrl, product.DataProvider,
                                                                    product.Detail, product.Reviews));

                _productsRepository.Add(product);
            }

            return await Commit(_productsRepository.UnitOfWork);
        }
    }
}

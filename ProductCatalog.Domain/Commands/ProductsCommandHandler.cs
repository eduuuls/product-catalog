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
    public class ProductsCommandHandler : CommandHandler, IRequestHandler<CreateNewProductsCommand, ValidationResult>
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

                var existingCategory = _productsRepository.List();

                //existingCategory.Wait();

                if (existingCategory != null)
                {
                    AddError($"Producto já existente: {product.Name}");
                    return;
                }

                product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.CategoryId, product.ExternalId, product.Name, 
                                                                    product.Description, product.Url, product.ImageUrl, product.DataProvider, 
                                                                        product.Detail, product.Reviews));

                _productsRepository.Add(product);
                
            });

            return await Commit(_productsRepository.UnitOfWork);
        }
    }
}

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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Commands
{
    public class CategoriesCommandHandler : CommandHandler, 
                                                IRequestHandler<CreateNewCategoriesCommand, ValidationResult>,
                                                IRequestHandler<CreateNewCategoryCommand, ValidationResult>,
                                                IRequestHandler<UpdateCategoryCommand, ValidationResult>
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public CategoriesCommandHandler(ILogger<ProductReviewsCommandHandler> logger, ICategoriesRepository categoriesRepository)
            : base(logger)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<ValidationResult> Handle(CreateNewCategoriesCommand message, CancellationToken cancellationToken)
        {
            this.ValidationResult = new ValidationResult();

            message.Commands.ForEach(c =>
            {
                if (!c.IsValid()) return;

                var category = new Category(Guid.NewGuid(), c.Name, c.Description, c.ImageUrl, c.IsActive, c.NumberOfProducts, c.DataProvider, c.Links);
                
                var categoryUpdatedEvent = new CategoryUpdatedEvent(category.Id, category.Name, category.Description, category.ImageUrl, category.IsActive,
                                                                            category.NumberOfProducts, category.DataProvider, category.Links);

                var task = _categoriesRepository.GetByKey(category.Name, category.DataProvider);

                task.Wait();

                var existingCategory = task.Result;

                if (existingCategory != null)
                {
                    existingCategory.Description = category.Description;
                    existingCategory.NumberOfProducts = category.NumberOfProducts;
                    existingCategory.ImageUrl = category.ImageUrl;
                    existingCategory.Links = category.Links;

                    category.AddDomainEvent(categoryUpdatedEvent);
                }
                else
                {
                    category.AddDomainEvent(categoryUpdatedEvent);

                    _categoriesRepository.Add(category);
                }
            });

            return await Commit(_categoriesRepository.UnitOfWork);
        }
        public async Task<ValidationResult> Handle(CreateNewCategoryCommand message, CancellationToken cancellationToken)
        {
            this.ValidationResult = new ValidationResult();

            if (!message.IsValid()) return message.ValidationResult;

            var category = new Category(Guid.NewGuid(), message.Name, message.Description, message.ImageUrl, message.IsActive, 
                                            message.NumberOfProducts, message.DataProvider, message.Links);
            
            var task = _categoriesRepository.GetByKey(category.Name, category.DataProvider);

            task.Wait();

            var existingCategory = task.Result;

            if (existingCategory != null)
            {
                existingCategory.Description = category.Description;
                existingCategory.NumberOfProducts = category.NumberOfProducts;
                existingCategory.ImageUrl = category.ImageUrl;
                existingCategory.Links = category.Links;
            }
            else
            {
                _categoriesRepository.Add(category);
            }

            return await Commit(_categoriesRepository.UnitOfWork);
        }
        public async Task<ValidationResult> Handle(UpdateCategoryCommand message, CancellationToken cancellationToken)
        {
            this.ValidationResult = new ValidationResult();

            if (!message.IsValid()) return message.ValidationResult;

            var category = await _categoriesRepository.GetById(message.Id);

            category.IsActive = message.IsActive;

            if (category.IsActive)
            {
                var activeLinks = category.Links.Where(l => l.IsActive);

                foreach (var link in activeLinks)
                {
                    category.AddDomainEvent(new CategoryUpdatedEvent(category.Id, category.Name, category.Description, category.ImageUrl, category.IsActive,
                                                                    category.NumberOfProducts, category.DataProvider, new List<CategoryLink>() { link }));
                }
            }

            return await Commit(_categoriesRepository.UnitOfWork);
        }
    }
}

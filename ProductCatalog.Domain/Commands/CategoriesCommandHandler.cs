using FluentValidation.Results;
using MediatR;
using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
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

        public CategoriesCommandHandler(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<ValidationResult> Handle(CreateNewCategoriesCommand message, CancellationToken cancellationToken)
        {
            message.Commands.ForEach(c =>
            {
                if (!c.IsValid()) return;

                var category = new Category(Guid.NewGuid(), c.Name, c.SubType, c.Description, c.Url, c.ImageUrl, c.IsActive, c.NumberOfProducts, c.DataProvider);

                var task = _categoriesRepository.GetByKey(category.Name, category.DataProvider);

                task.Wait();

                var existingCategory = task.Result;

                if (existingCategory != null)
                {
                    existingCategory.Description = category.Description;
                    existingCategory.NumberOfProducts = category.NumberOfProducts;
                    existingCategory.ImageUrl = category.ImageUrl;
                    existingCategory.Url = category.Url;

                    category.AddDomainEvent(new CategoryUpdatedEvent(category.Id, category.Name, category.SubType, category.Description, category.Url,
                                                                    category.ImageUrl, category.IsActive, category.NumberOfProducts, category.DataProvider));
                }
                else
                {
                    category.AddDomainEvent(new CategoryCreatedEvent(category.Id, category.Name, category.SubType, category.Description, category.Url,
                                                                    category.ImageUrl, category.IsActive, category.NumberOfProducts, category.DataProvider));

                    _categoriesRepository.Add(category);
                }
            });

            return await Commit(_categoriesRepository.UnitOfWork);
        }
        public async Task<ValidationResult> Handle(CreateNewCategoryCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid()) return message.ValidationResult;

            var category = new Category(Guid.NewGuid(), message.Name, message.SubType, message.Description, message.Url,
                                            message.ImageUrl, message.IsActive, message.NumberOfProducts, message.DataProvider);

            var task = _categoriesRepository.GetByKey(category.Name, category.DataProvider);

            task.Wait();

            var existingCategory = task.Result;

            if (existingCategory != null)
            {
                existingCategory.Description = category.Description;
                existingCategory.NumberOfProducts = category.NumberOfProducts;
                existingCategory.ImageUrl = category.ImageUrl;
                existingCategory.Url = category.Url;

                category.AddDomainEvent(new CategoryUpdatedEvent(category.Id, category.Name, category.SubType, category.Description, category.Url,
                                                                category.ImageUrl, category.IsActive, category.NumberOfProducts, category.DataProvider));
            }
            else
            {
                category.AddDomainEvent(new CategoryCreatedEvent(category.Id, category.Name, category.SubType, category.Description, category.Url,
                                                                category.ImageUrl, category.IsActive, category.NumberOfProducts, category.DataProvider));

                _categoriesRepository.Add(category);
            }

            return await Commit(_categoriesRepository.UnitOfWork);
        }
        public async Task<ValidationResult> Handle(UpdateCategoryCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid()) return message.ValidationResult;

            var category = await _categoriesRepository.GetById(message.Id);

            category.IsActive = message.IsActive;

            category.AddDomainEvent(new CategoryUpdatedEvent(category.Id, category.Name, category.SubType, category.Description,
                                            category.Url, category.ImageUrl, category.IsActive, category.NumberOfProducts, category.DataProvider));

            return await Commit(_categoriesRepository.UnitOfWork);
        }
    }
}

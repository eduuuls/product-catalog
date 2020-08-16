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
    public class CategoryCommandHandler: CommandHandler, 
                    IRequestHandler<CreateNewCategoryCommand, ValidationResult>,
                    IRequestHandler<UpdateCategoryCommand, ValidationResult>
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public CategoryCommandHandler(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public async Task<ValidationResult> Handle(CreateNewCategoryCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid()) return message.ValidationResult;

            var category = new Category(Guid.NewGuid(), message.Name, message.Description, message.Url, 
                                            message.ImageUrl, message.IsActive, message.NumberOfProducts, message.DataProvider);

            if (await _categoriesRepository.GetByKey(category.Name, category.DataProvider) != null)
            {
                AddError("Já existe uma categoria com teste nome.");
                return ValidationResult;
            }

            category.AddDomainEvent(new CategoryCreatedEvent(category.Id, category.Name, category.Description,
                                            category.Url, category.ImageUrl, category.IsActive, category.NumberOfProducts, 
                                                category.DataProvider));

            _categoriesRepository.Add(category);

            return await Commit(_categoriesRepository.UnitOfWork);
        }

        public async Task<ValidationResult> Handle(UpdateCategoryCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid()) return message.ValidationResult;

            var category = await _categoriesRepository.GetById(message.Id);

            category.IsActive = message.IsActive;

            category.AddDomainEvent(new CategoryUpdatedEvent(category.Id, category.Name, category.Description,
                                            category.Url, category.ImageUrl, category.IsActive, category.NumberOfProducts,
                                                category.DataProvider));

            _categoriesRepository.Update(category);

            return await Commit(_categoriesRepository.UnitOfWork);
        }
    }
}

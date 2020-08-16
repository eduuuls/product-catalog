using AutoMapper;
using FluentValidation.Results;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.ViewModels;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Infra.CrossCutting.Bus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductCatalog.Application.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly IMediatorHandler _mediatorHandler;
        private readonly ICategoriesRepository _categoriesRepository;
        public CategoryService(IMapper mapper, IMediatorHandler mediator, ICategoriesRepository categoriesRepository)
        {
            _mapper = mapper;
            _mediatorHandler = mediator;
            _categoriesRepository = categoriesRepository;
        }

        public async Task<ValidationResult> Create(CategoryViewModel categoryViewModel)
        {
            var registerCommand = _mapper.Map<CreateNewCategoryCommand>(categoryViewModel);
            return await _mediatorHandler.SendCommand(registerCommand);
        }

        public async Task<List<CategoryViewModel>> GetByDataProvider(DataProvider dataProvider, bool onlyActive)
        {
            return _mapper.Map<List<CategoryViewModel>>(await _categoriesRepository.GetByProvider(dataProvider, onlyActive));
        }

        public async Task<CategoryViewModel> GetById(Guid id)
        {
            return _mapper.Map<CategoryViewModel>(await _categoriesRepository.GetById(id));
        }

        public async Task<ValidationResult> Update(CategoryViewModel categoryViewModel)
        {
            var registerCommand = _mapper.Map<UpdateCategoryCommand>(categoryViewModel);
            return await _mediatorHandler.SendCommand(registerCommand);
        }
    }
}

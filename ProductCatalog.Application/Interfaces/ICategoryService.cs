using FluentValidation.Results;
using ProductCatalog.Application.ViewModels;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<ValidationResult> Create(CategoryViewModel categoryViewModel);
        Task<List<CategoryViewModel>> GetByDataProvider(DataProvider dataProvider, bool onlyActive);
        Task<CategoryViewModel> GetById(Guid id);
        Task<ValidationResult> Update(CategoryViewModel categoryViewModel);
    }
}

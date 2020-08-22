using AutoMapper;
using ProductCatalog.Application.ViewModels;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Application.AutoMapper
{
    public class DomainToViewModelMapping : Profile
    {
        public DomainToViewModelMapping()
        {
            CreateMap<Product, ProductViewModel>();
            CreateMap<ProductDetail, ProductDetailViewModel>();
            CreateMap<ProductReview, ProductReviewViewModel>();
            CreateMap<CreateNewCategoryCommand, CategoryViewModel>();
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CreateNewCategoryCommand, CategoryViewModel>();
            CreateMap<UpdateCategoryCommand, CategoryViewModel>();
            CreateMap<CategoryCreatedEvent, CategoryViewModel>();
            CreateMap<CategoryUpdatedEvent, CategoryViewModel>();
            CreateMap<ProductCreatedUpdatedEvent, ProductViewModel>();
            
        }
    }
}

using AutoMapper;
using ProductCatalog.Application.ViewModels;
using ProductCatalog.Domain.Commands;
using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Application.AutoMapper
{
    public class ViewModelToDomainMapping: Profile
    {
        public ViewModelToDomainMapping()
        {
            CreateMap<ProductViewModel, Product>();
            CreateMap<ProductDetailViewModel, ProductDetail>();
            CreateMap<ProductReviewViewModel, ProductReview>();
            CreateMap<CategoryViewModel, CreateNewCategoryCommand>();
            CreateMap<CategoryViewModel, UpdateCategoryCommand>();
            CreateMap<CategoryViewModel, Category>();
            CreateMap<CategoryLinkViewModel, CategoryLink>();
        }
    }
}

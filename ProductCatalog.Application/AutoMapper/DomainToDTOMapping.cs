﻿using AutoMapper;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.AutoMapper
{
    public class DomainToDtoMapping : Profile
    {
        public DomainToDtoMapping()
        {
            CreateMap<ProductReview, ProductReviewDTO>();
        }
    }
}

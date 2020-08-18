using AutoMapper;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.AutoMapper
{
    public class DtoToDomainMapping : Profile
    {
        public DtoToDomainMapping()
        {
            CreateMap<ProductReviewDTO, ProductReview>();
        }
    }
}

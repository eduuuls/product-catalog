using AutoMapper;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.AutoMapper
{
    public class DomainToDTOMapping : Profile
    {
        public DomainToDTOMapping()
        {
            CreateMap<ProductReview, ProductReviewDTO>();
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Domain.Interfaces.UoW;
using ProductCatalog.Infra.Data.Persistance.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.Persistance.Repositories
{
    public class ProductsRepository : Repository<Product>, IProductsRepository
    {
        public IUnitOfWork UnitOfWork => _unitOfWork;

        public ProductsRepository(ProductsCatalogDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {

        }

        public async Task<Product> GetByKey(Guid categoryId, string externalId, DataProvider dataProvider)
        {
            var product = await List().FirstOrDefaultAsync(p => p.CategoryId == categoryId
                                                                    && p.ExternalId == externalId
                                                                        && p.DataProvider == dataProvider);

            return product;
        }
    }
}

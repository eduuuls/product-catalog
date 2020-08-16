﻿using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Domain.Interfaces.UoW;
using ProductCatalog.Infra.Data.Persistance.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Infra.Data.Persistance.Repositories
{
    public class ProductsDetailRepository : Repository<ProductDetail>, IProductsDetailRepository
    {
        public ProductsDetailRepository(ProductsCatalogDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {

        }
    }
}
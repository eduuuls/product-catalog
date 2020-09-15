using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProductCatalog.Domain.DTO.Query;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces.Queries;
using Slapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.Persistance.Queries
{
    public class CategoriesQuery : ICategoriesQuery
    {
        private readonly string _connectionString;

        public CategoriesQuery(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("productCatalog");
        }
       
    }
}

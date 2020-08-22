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
    public class ProductsQuery : IProductsQuery
    {
        private readonly string _connectionString;

        public ProductsQuery(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("productCatalog");
        }
        public async Task<ProductQueryDTO> GetProductFullDataById(Guid id)
        {
            ProductQueryDTO product = null;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = $@"SELECT P.Id, P.Name, P.CategoryId, P.ImageUrl, P.RelevancePoints, P.DataProvider, 
                                         C.Id AS Category_CategoryId, C.Name AS Category_Name, PD.Id AS Detail_DetailId, PD.BarCode AS Detail_BarCode, 
                                         PD.Brand AS Detail_Brand, PD.Manufacturer AS Detail_Manufacturer, PD.Model AS Detail_Model,
                                         PD.ReferenceModel AS Detail_ReferenceModel, PD.Supplier AS Detail_Supplier, PD.OtherSpecs AS Detail_OtherSpecs,
		                                 PR.Id AS Reviews_ReviewId, PR.ExternalId AS Reviews_ExternalId, PR.Date AS Reviews_Date, 
                                         PR.ProductId AS Reviews_ProductId, PR.Reviewer AS Reviews_Reviewer, PR.Title AS Reviews_Title, 
                                         PR.Text AS Reviews_Text, PR.Stars AS Reviews_Stars, PR.IsRecommended AS Reviews_IsRecommended
	                                FROM [dbo].[Products] P
                                    INNER JOIN [dbo].[ProductsDetail] PD 
	                                    ON P.Id = PD.ProductId
                                    INNER JOIN [dbo].[Categories] C 
	                                    ON P.CategoryId = C.Id
                                    LEFT JOIN [dbo].[ProductsReviews] PR
	                                    ON P.Id = PR.ProductId
                                    WHERE P.Id = @Id
                                    ORDER BY P.RelevancePoints DESC";

                var result = await conexao.QueryAsync<dynamic>(query, new { Id = id });

                AutoMapper.Configuration.AddIdentifier(typeof(ProductQueryDTO), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(Category), "CategoryId");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductDetail), "DetailId");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductReview), "ReviewId");

                product = AutoMapper.MapDynamic<Product>(result)
                                    .Select(r => new ProductQueryDTO
                                    {
                                        Id = r.Id,
                                        Name = r.Name,
                                        Category = r.Category,
                                        DataProvider = r.DataProvider,
                                        RelevancePoints = r.RelevancePoints,
                                        Reviews = r.Reviews,
                                        ImageUrl = r.ImageUrl,
                                        BarCode = r.Detail.BarCode,
                                        Brand = r.Detail.Brand,
                                        Manufacturer = r.Detail.Manufacturer,
                                        Model = r.Detail.Model,
                                        ReferenceModel = r.Detail.ReferenceModel,
                                        Supplier = r.Detail.Supplier,
                                        OtherSpecs = r.Detail.OtherSpecs
                                    }).FirstOrDefault();
            }

            return product;
        }
        public async Task<IEnumerable<Product>> GetProductByParameter(string name, string value)
        {
            IEnumerable<Product> products = null;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return products;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = $@"SELECT P.Id, P.Name, P.CategoryId, P.ImageUrl, P.RelevancePoints, P.DataProvider, 
                                         PD.Id AS Detail_DetailId, PD.BarCode AS Detail_BarCode, PD.Brand AS Detail_Brand, 
                                         PD.Manufacturer AS Detail_Manufacturer, PD.Model AS Detail_Model, PD.ReferenceModel AS Detail_ReferenceModel, 
                                         PD.Supplier AS Detail_Supplier, PD.OtherSpecs AS Detail_OtherSpecs, PR.Id AS Reviews_ReviewId, 
                                         PR.ExternalId AS Reviews_ExternalId, PR.Date AS Reviews_Date, PR.ProductId AS Reviews_ProductId, 
                                         PR.Reviewer AS Reviews_Reviewer, PR.Title AS Reviews_Title, PR.Text AS Reviews_Text, PR.Stars AS Reviews_Stars, 
                                         PR.IsRecommended AS Reviews_IsRecommended
	                                FROM [dbo].[Products] P
                                    INNER JOIN [dbo].[ProductsDetail] PD 
	                                    ON P.Id = PD.ProductId
                                    LEFT JOIN [dbo].[ProductsReviews] PR
	                                    ON P.Id = PR.ProductId
                                    WHERE PD.{name} = @Parameter
                                    ORDER BY P.RelevancePoints DESC";

                var result = await conexao.QueryAsync<dynamic>(query, new { Parameter = value });

                AutoMapper.Configuration.AddIdentifier(typeof(Product), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductDetail), "DetailId");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductReview), "ReviewId");

                products = AutoMapper.MapDynamic<Product>(result).ToList();
            }

            return products;
        }
        public async Task<IEnumerable<MergeableProductDTO>> GetMergeableByModel()
        {
            IEnumerable<MergeableProductDTO> result = null;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = @"SELECT PD.Model
                                    FROM [dbo].[ProductsDetail] PD
                                    WHERE PD.Model IS NOT NULL
                                    GROUP BY PD.Model
                                    HAVING COUNT(*) > 1";

                result = await conexao.QueryAsync<MergeableProductDTO>(query);
            }

            return result;
        }
        public async Task<IEnumerable<MergeableProductDTO>> GetMergeableByReferenceModel()
        {
            IEnumerable<MergeableProductDTO> result = null;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = @"SELECT PD.ReferenceModel
                                    FROM [dbo].[ProductsDetail] PD
                                    WHERE PD.ReferenceModel IS NOT NULL
                                    GROUP BY PD.ReferenceModel
                                    HAVING COUNT(*) > 1";

                result = await conexao.QueryAsync<MergeableProductDTO>(query);
            }

            return result;
        }
        public async Task<IEnumerable<MergeableProductDTO>> GetMergeableByBarCode()
        {
            IEnumerable<MergeableProductDTO> result = null;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = @"SELECT PD.BarCode, COUNT(PD.BarCode) 
                                    FROM [dbo].[ProductsDetail] PD
                                    GROUP BY PD.BarCode
                                    HAVING COUNT(PD.BarCode) > 1";

                result = await conexao.QueryAsync<MergeableProductDTO>(query);
            }

            return result;
        }
    }
}

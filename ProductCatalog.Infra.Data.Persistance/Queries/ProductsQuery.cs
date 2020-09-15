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
        public async Task<Product> GetProductDataById(Guid id)
        {
            Product product = null;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = $@"SELECT P.Id, P.Name, P.CategoryId, P.ImageUrl, P.RelevancePoints, PD.Id AS Detail_Id, 
                                        PD.BarCode AS Detail_BarCode, PD.Brand AS Detail_Brand, PD.Manufacturer AS Detail_Manufacturer, 
                                        PD.Model AS Detail_Model, PD.ReferenceModel AS Detail_ReferenceModel, PD.Supplier AS Detail_Supplier, 
                                        PD.OtherSpecs AS Detail_OtherSpecs
	                            FROM [dbo].[Products] P
                                INNER JOIN [dbo].[ProductsDetail] PD 
	                                ON P.Id = PD.ProductId
                                    WHERE P.Id = @Id
                                        AND P.RelevancePoints > 5
                                    ORDER BY P.RelevancePoints DESC";

                var result = await conexao.QueryAsync<dynamic>(query, new { Id = id });

                AutoMapper.Configuration.AddIdentifier(typeof(Product), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductDetail), "Id");

                product = AutoMapper.MapDynamic<Product>(result).FirstOrDefault();
            }

            return product;
        }
        public async Task<ProductQueryDTO> GetProductFullDataById(Guid id)
        {
            ProductQueryDTO product = null;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = $@"SELECT P.Id, P.Name, P.CategoryId, P.ImageUrl, P.RelevancePoints,
                                        C.Id AS Category_Id, C.Name AS Category_Name, PD.Id AS Detail_Id, PD.BarCode AS Detail_BarCode, 
                                        PD.Brand AS Detail_Brand, PD.Manufacturer AS Detail_Manufacturer, PD.Model AS Detail_Model,
                                        PD.ReferenceModel AS Detail_ReferenceModel, PD.Supplier AS Detail_Supplier, PD.OtherSpecs AS Detail_OtherSpecs,
		                                PR.Id AS Reviews_Id, PR.ExternalId AS Reviews_ExternalId, PR.Date AS Reviews_Date, 
                                        PR.ProductId AS Reviews_ProductId, PR.Reviewer AS Reviews_Reviewer, PR.Title AS Reviews_Title, 
                                        PR.Text AS Reviews_Text, PR.Stars AS Reviews_Stars, PR.IsRecommended AS Reviews_IsRecommended,
			                            REC.Recomendations,DREC.Disrecomendations, FS.FiveStars, FRS.FourStars,TRS.ThreeStars, TS.TwoStars, OS.OneStar
	                            FROM [dbo].[Products] P
                                INNER JOIN [dbo].[ProductsDetail] PD 
	                                ON P.Id = PD.ProductId
                                INNER JOIN [dbo].[Categories] C 
	                                ON P.CategoryId = C.Id
                                LEFT JOIN [dbo].[ProductsReviews] PR
	                                ON P.Id = PR.ProductId
	                            LEFT JOIN (SELECT ProductId, COUNT(ID) Recomendations FROM [dbo].[ProductsReviews] 
				                            WHERE  IsRecommended = 1
				                            GROUP BY ProductId) AS REC ON REC.ProductId = P.Id
	                            LEFT JOIN (SELECT ProductId, COUNT(ID) Disrecomendations FROM [dbo].[ProductsReviews] 
				                            WHERE  IsRecommended = 0
				                            GROUP BY ProductId) AS DREC ON DREC.ProductId = P.Id
	                            LEFT JOIN (SELECT ProductId, COUNT(ID) FiveStars FROM [dbo].[ProductsReviews] 
				                            WHERE Stars = 5
				                            GROUP BY ProductId) AS FS ON FS.ProductId = P.Id
	                            LEFT JOIN (SELECT ProductId, COUNT(ID) FourStars FROM [dbo].[ProductsReviews]
				                            WHERE  Stars = 4
				                            GROUP BY ProductId) AS FRS ON FRS.ProductId = P.Id
	                            LEFT JOIN (SELECT ProductId, COUNT(ID) ThreeStars FROM [dbo].[ProductsReviews]
				                            WHERE  Stars = 3
				                            GROUP BY ProductId) AS TRS ON TRS.ProductId = P.Id
	                            LEFT JOIN (SELECT ProductId, COUNT(ID) TwoStars FROM [dbo].[ProductsReviews]
				                            WHERE  Stars = 2
				                            GROUP BY ProductId) AS TS ON TS.ProductId = P.Id
	                            LEFT JOIN (SELECT ProductId, COUNT(ID) OneStar FROM [dbo].[ProductsReviews]
				                            WHERE  Stars = 1
				                            GROUP BY ProductId) AS OS ON OS.ProductId = P.Id
                                    WHERE P.Id = @Id
                                        AND P.RelevancePoints > 5
                                    ORDER BY P.RelevancePoints DESC";

                var result = await conexao.QueryAsync<dynamic>(query, new { Id = id });

                AutoMapper.Configuration.AddIdentifier(typeof(ProductQueryDTO), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(Category), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductDetail), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductReview), "Id");

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
        public async Task<IEnumerable<Product>> GetProductsToMerge(string model, string referenceModel, string brand, string manufacturer)
        {
            IEnumerable<Product> products = null;

            if ((string.IsNullOrEmpty(model) && string.IsNullOrEmpty(referenceModel))
                    || (string.IsNullOrEmpty(brand) && string.IsNullOrEmpty(manufacturer)))
                return products;

            using (SqlConnection conexao = new SqlConnection(_connectionString))
            {
                string query = $@"SELECT P.Id, P.Name, P.CategoryId, P.ImageUrl, P.RelevancePoints, P.DataProvider, 
                                         PD.Id AS Detail_Id, PD.BarCode AS Detail_BarCode, PD.Brand AS Detail_Brand, 
                                         PD.Manufacturer AS Detail_Manufacturer, PD.Model AS Detail_Model, PD.ReferenceModel AS Detail_ReferenceModel, 
                                         PD.Supplier AS Detail_Supplier, PD.OtherSpecs AS Detail_OtherSpecs, PR.Id AS Reviews_Id, 
                                         PR.ExternalId AS Reviews_ExternalId, PR.Date AS Reviews_Date, PR.ProductId AS Reviews_ProductId, 
                                         PR.Reviewer AS Reviews_Reviewer, PR.Title AS Reviews_Title, PR.Text AS Reviews_Text, PR.Stars AS Reviews_Stars, 
                                         PR.IsRecommended AS Reviews_IsRecommended
	                                FROM [dbo].[Products] P
                                    INNER JOIN [dbo].[ProductsDetail] PD 
	                                    ON P.Id = PD.ProductId
                                    LEFT JOIN [dbo].[ProductsReviews] PR
	                                    ON P.Id = PR.ProductId
                                    WHERE (Upper(PD.Model) = Upper(@Model) OR Upper(PD.ReferenceModel) = Upper(@ReferenceModel))
	                                  AND (Upper(PD.Brand) = Upper(@Brand) OR Upper(PD.Manufacturer) = Upper(@Manufacturer))
                                    ORDER BY P.RelevancePoints DESC";

                var result = await conexao.QueryAsync<dynamic>(query, new { Model = model, ReferenceModel = referenceModel, Brand = brand, Manufacturer = manufacturer });

                AutoMapper.Configuration.AddIdentifier(typeof(Product), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductDetail), "Id");
                AutoMapper.Configuration.AddIdentifier(typeof(ProductReview), "Id");

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

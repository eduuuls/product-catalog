using ProductCatalog.Domain.DTO.Query;
using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.Queries
{
    public interface IProductsQuery
    {
        Task<ProductQueryDTO> GetProductFullDataById(Guid id);
        Task<IEnumerable<Product>> GetProductByParameter(string name, string value);
        Task<IEnumerable<MergeableProductDTO>> GetMergeableByModel();
        Task<IEnumerable<MergeableProductDTO>> GetMergeableByReferenceModel();
        Task<IEnumerable<MergeableProductDTO>> GetMergeableByBarCode();
    }
}

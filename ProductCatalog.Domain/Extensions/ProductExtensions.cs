using Newtonsoft.Json;
using ProductCatalog.Domain.Entities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProductCatalog.Domain.Extensions
{
    public static class ProductExtensions
    {
        public static int GetRelevancePoints(this ProductDetail productDetail)
        {
            int relevancePoints = 0;

            if (!string.IsNullOrEmpty(productDetail.Model))
                relevancePoints += 6;

            if (!string.IsNullOrEmpty(productDetail.ReferenceModel))
                relevancePoints += 5;

            if (!string.IsNullOrEmpty(productDetail.BarCode))
                relevancePoints += 4;

            if (!string.IsNullOrEmpty(productDetail.Brand))
                relevancePoints += 3;

            if (!string.IsNullOrEmpty(productDetail.Manufacturer))
                relevancePoints += 2;

            if (!string.IsNullOrEmpty(productDetail.Supplier))
                relevancePoints += 1;

            return relevancePoints;
        }
        public static Product MergeProducts(this Product main, Product other)
        {
            if (main == null || other == null)
                return main;

            if (!string.IsNullOrEmpty(other.Detail.BarCode))
            {
                var meBarCodes = main.Detail.BarCode.Split(",");
                var otherBarCodes = other.Detail.BarCode.Split(",");

                main.Detail.BarCode = string.Join(",", meBarCodes.Union(otherBarCodes).Distinct());
            }

            if (!string.IsNullOrEmpty(other.Detail.Brand))
                main.Detail.Brand = !string.IsNullOrEmpty(main.Detail.Brand) ? main.Detail.Brand : other.Detail.Brand;

            if (!string.IsNullOrEmpty(other.Detail.Manufacturer))
                main.Detail.Manufacturer = !string.IsNullOrEmpty(main.Detail.Manufacturer) ? main.Detail.Brand : other.Detail.Manufacturer;

            if (!string.IsNullOrEmpty(other.Detail.ReferenceModel))
                main.Detail.ReferenceModel = !string.IsNullOrEmpty(main.Detail.ReferenceModel) ? main.Detail.ReferenceModel : other.Detail.ReferenceModel;

            if (!string.IsNullOrEmpty(other.Detail.Supplier))
                main.Detail.Supplier = !string.IsNullOrEmpty(main.Detail.Supplier) ? main.Detail.Supplier : other.Detail.Supplier;

            if (!string.IsNullOrEmpty(other.Detail.OtherSpecs))
                main.Detail.OtherSpecs = main.Detail.OtherSpecs.MergeOtherSpecs(other.Detail.OtherSpecs);

            return main;
        }
        public static string MergeOtherSpecs(this string main, string other)
        {
            try
            {
                var mainDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(main);
                var otherDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(other);

                var diffDic = otherDictionary.Where(o => !mainDictionary.ContainsKey(o.Key));

                var mergeResult = mainDictionary.Concat(diffDic).ToDictionary(k=> k.Key, v=> v.Value);

                return JsonConvert.SerializeObject(mergeResult, Newtonsoft.Json.Formatting.None);
            }
            catch
            {
                return main;
            }
        }
    }
}

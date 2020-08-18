using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ProductCatalog.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using ProductCatalog.Domain.DTO;

namespace ProductCatalog.Infra.Data.ExternalServices
{
    public class MercadoLivreExternalService : Base.ExternalService, IMercadoLivreExternalService
    {
        private string _websiteUrl = "https://lista.mercadolivre.com.br/";
        private string _reviewsUrl = "https://www.mercadolivre.com.br/noindex/catalog/reviews/search?itemId=@id&offset=0&limit=50";
        public MercadoLivreExternalService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {

        }

        public async Task<ProductDTO[]> SearchProducts(string search)
        {
            List<ProductDTO> products = new List<ProductDTO>();
            List<Task<ProductDTO>> tasks = new List<Task<ProductDTO>>();

            var htmlResposta = await ExecuteHtmlRequest($"{_websiteUrl}/{search}");
            
            var resultElements = htmlResposta.CssSelect("li[class^=results-item]");

            await Task.Run(() =>
            {
                Parallel.ForEach(resultElements, element =>
                {
                    var result = ConvertHtmlToProduct(element);

                    if (result != null)
                        tasks.Add(result);
                });
            });

            return Task.WhenAll(tasks).Result
                                        .Where(r => r != null)
                                            .ToArray();

            #region Test Code
            //foreach (var element in resultElements)
            //{
            //    var result = await ConvertHtmlToProduct(element);

            //    if (result != null)
            //        products.Add(result);
            //}
            //return products.ToArray();
            #endregion
        }

        private async Task<ProductDTO> ConvertHtmlToProduct(HtmlNode htmlNode)
        {
            ProductDTO product = new ProductDTO();

            var image = htmlNode.CssSelect("a[class~=item-image]")
                                    .CssSelect("img").FirstOrDefault();

            if (image != null)
                product.ImageUrl = image.GetAttributeValue("data-src");

            var name = htmlNode.CssSelect("h2[class^=item__title]")
                                    .CssSelect("span[class=main-title]")
                                        .FirstOrDefault();

            if (name != null)
                product.Name = name.InnerText.Trim();

            var productLink = htmlNode.CssSelect("h2[class^=item__title]")
                                            .CssSelect("a")
                                                .FirstOrDefault();

            if (productLink != null)
            {
                var detailUrl = productLink.GetAttributeValue("href");
                
                product.Url = detailUrl;

                var startIdIndex = detailUrl.IndexOf("MLB");
                var endIdIndex = detailUrl.IndexOf("?", startIdIndex);

                if(endIdIndex < 0)
                    endIdIndex = detailUrl.IndexOf("#", startIdIndex);

                product.ExternalId = detailUrl.Substring(startIdIndex, endIdIndex - startIdIndex);

                return product;

                //var isValidProduct = await FillProductDetail(detailUrl, product);

                //if (isValidProduct)
                //    return product;
            }

            return null;
        }

        private async Task<bool> FillProductDetail(string detailUrl, ProductDTO product)
        {
            product.Reviews = new List<ProductReviewDTO>();
            

            var detailResponse = await ExecuteHtmlRequest(detailUrl);

            var techSpecs = detailResponse.CssSelect("div[class=ui-pdp-specs__table]")
                                                .CssSelect("table[class=andes-table]")
                                                    .CssSelect("tr[class=andes-table__row]")
                                                        .Where(t => t.ChildNodes.Count == 2)
                                                            .Select(t =>
                                                            {
                                                                var key = t.ChildNodes[0].InnerText?.Trim();
                                                                var value = t.ChildNodes[1].FirstChild.InnerText?.Trim();

                                                                return new KeyValuePair<string, string>(key, value);
                                                            }).ToList();

            var hasUniqueConstraintInfo = techSpecs.Any(t => t.Key.ToUpper().Contains("MODELO"));

            if (!hasUniqueConstraintInfo)
                return false;

            await Task.Run(() =>
            {
                var model = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("MODELO"));
                product.Model = model.Value;
                techSpecs.Remove(model);

                var brand = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("MARCA"));
                product.Brand = brand.Value;
                techSpecs.Remove(brand);

                var code = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("CÓDIGO"));
                product.Code = code.Value;
                techSpecs.Remove(code);

                var manufacturer = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("FABRICANTE"));
                product.Manufacturer = manufacturer.Value;
                techSpecs.Remove(manufacturer);

                var referenceModel = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("MODELO ALFANUMÉRICO"));
                product.ReferenceModel = referenceModel.Value;
                techSpecs.Remove(referenceModel);

                var supplier = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("FORNECEDOR"));
                product.Supplier = supplier.Value;
                techSpecs.Remove(supplier);

                ConcurrentDictionary<string, string> specs = new ConcurrentDictionary<string, string>();

                Parallel.ForEach(techSpecs, spec => specs.TryAdd(spec.Key, spec.Value));
                product.OtherSpecs = JsonConvert.SerializeObject(specs);
            });
            
            await Task.Run(async () =>
            {
                var reviewsUrl = _reviewsUrl.Replace("@id", product.ExternalId);

                var reviewsResponse = await ExecuteHtmlRequest(reviewsUrl);

                var reviews = reviewsResponse.CssSelect("article[class=review-element]");

                if (reviews != null)
                {
                    ConcurrentStack<ProductReviewDTO> reviewsToAdd = new ConcurrentStack<ProductReviewDTO>();

                    Parallel.ForEach(reviews, review =>
                    {
                        ProductReviewDTO productReview = new ProductReviewDTO();

                        productReview.Reviewer = "E-Consumidor";

                        var title = review.CssSelect("label").FirstOrDefault();

                        if (title != null)
                            productReview.Title = title.InnerText;

                        var text = review.CssSelect("p").FirstOrDefault();

                        if (title != null)
                            productReview.Text = text.InnerText.Trim();

                        reviewsToAdd.Push(productReview);
                    });

                    product.Reviews = reviewsToAdd.ToList();
                }
            });

            return true;
        }
    }
}

using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using ProductCatalog.Domain.DTO;

namespace ProductCatalog.Infra.Data.ExternalServices
{
    public class BuscapeExternalService : Base.ExternalService, IBuscapeExternalService
    {
        private string _urlBase = "https://www.buscape.com.br";
        public BuscapeExternalService(IHttpClientFactory clientFactory) : base(clientFactory)
        {

        }

        public async Task<ProductDTO[]> SearchProducts(string search)
        {
            List<Product> products = new List<Product>();
            List<Task<ProductDTO>> tasks = new List<Task<ProductDTO>>();

            var htmlResposta = await ExecuteHtmlRequest($"{_urlBase}/search?q={search}");

            var resultElements = htmlResposta.CssSelect("div[id=pageSearchResultsBody]")
                                        .CssSelect("div[class=SearchPage_searchList__1rdGp]")
                                            .CssSelect(".card--prod");
            await Task.Run(() =>
            {
                Parallel.ForEach(resultElements, element =>
                {
                    var result = ConvertHtmlToProduct(element);

                    tasks.Add(result);
                });
            });

            return Task.WhenAll(tasks).Result;
        }

        private async Task<ProductDTO> ConvertHtmlToProduct(HtmlNode htmlNode)
        {
            var product = new ProductDTO();

            product.ExternalId = htmlNode.GetAttributeValue("data-id");

            var nome = htmlNode.CssSelect(".cardBody")
                                    .CssSelect(".name")
                                        .FirstOrDefault();

            if (nome != null)
                product.Name = nome.InnerText;

            var urlFoto = htmlNode.CssSelect(".image")
                                        .FirstOrDefault();

            if (urlFoto != null && urlFoto.HasAttributes)
                product.ImageUrl = urlFoto.GetAttributeValue("src");

            if (nome.HasAttributes)
            {
                var productUrl = nome.GetAttributeValue("href");

                product.Url = $"{_urlBase}{productUrl}";

                //var isValidProduct = await FillProductDetail(detalheUrl, product);

                //if (isValidProduct)
                //    return product;
            }

            return product;
        }

        private async Task<bool> FillProductDetail(string productUrl, ProductDTO product)
        {
            product.Reviews = new List<ProductReviewDTO>();

            var response = await ExecuteHtmlRequest(productUrl);

            var techSpecs = response.CssSelect(".tech-spec-table")
                                                .CssSelect(".ti").ToList();

            var reviews = response.CssSelect("#comments-list")
                                            .CssSelect(".review");

            await Task.Run(() =>
            {
                ConcurrentDictionary<string, string> specification = new ConcurrentDictionary<string, string>();

                Parallel.ForEach(techSpecs, spec =>
                {
                    if (spec.ChildNodes.Count == 2)
                        specification.TryAdd(spec.ChildNodes[0].InnerText?.Trim(), spec.ChildNodes[1].InnerText?.Trim());
                });

                product.OtherSpecs = JsonConvert.SerializeObject(specification);
            });

            await Task.Run(() =>
            {
                ConcurrentStack<ProductReviewDTO> reviewsToAdd = new ConcurrentStack<ProductReviewDTO>();

                Parallel.ForEach(reviews, review =>
                {
                    ProductReviewDTO productReview = new ProductReviewDTO();

                    productReview.Reviewer = "E-Consumidor";

                    var data = review.CssSelect(".date")?.FirstOrDefault()?.InnerText.Replace("em", "").Trim();

                    if (!string.IsNullOrEmpty(data))
                        productReview.Date = Convert.ToDateTime(data, CultureInfo.GetCultureInfo("pt-BR"));

                    productReview.Title = HttpUtility.HtmlDecode(review.CssSelect(".title")?.FirstOrDefault()?.InnerText);
                    productReview.Text = HttpUtility.HtmlDecode(review.CssSelect(".text")?.FirstOrDefault()?.InnerText);
                    productReview.Result = review.CssSelect(".recommendation")?.FirstOrDefault()?.InnerText.Trim();

                    var estrelas = review.CssSelect("span[class*='star_']")?
                                            .FirstOrDefault()?
                                                .GetAttributeValue("class")?
                                                    .LastOrDefault();

                    if (estrelas.HasValue)
                        productReview.Stars = Convert.ToInt16(estrelas.ToString());

                    reviewsToAdd.Push(productReview);
                });

                product.Reviews = reviewsToAdd.ToList();
            });

            return true;
        }
    }
}

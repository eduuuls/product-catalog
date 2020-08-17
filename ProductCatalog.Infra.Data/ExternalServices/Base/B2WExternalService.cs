using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using ProductCatalog.Domain.DTO;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.ExternalServices.Base;
using ProductCatalog.Infra.Data.Configuration;
using ScrapySharp.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.ExternalServices.Base
{
    public abstract class B2WExternalService : ExternalService
    {
        private readonly WebsiteConfiguration _websiteConfiguration;
        private readonly ChromeOptions _chromeOptions;
        private readonly DataProvider _dataProvider;
        
        public B2WExternalService(IHttpClientFactory httpClientFactory,
                                    ILogger logger,
                                    ChromeOptions chromeOptions,
                                        IOptions<ExternalServicesConfiguraiton> externalServicesConfiguraiton,
                                            DataProvider dataProvider) : base(httpClientFactory, logger, "B2WClient")
        {
            _dataProvider = dataProvider;
            _chromeOptions = chromeOptions;
            _websiteConfiguration = externalServicesConfiguraiton.Value?
                                            .WebsiteConfigurations?
                                                .FirstOrDefault(w => w.Key == dataProvider.ToString());
        }
        public async Task<List<CategoryDTO>> GetProductCategories()
        {
            HtmlDocument document = new HtmlDocument();
            List<HtmlNode> htmlNodes = new List<HtmlNode>();

            string requestUrl = string.Concat(_websiteConfiguration.BaseAdress,
                                                       "/", _websiteConfiguration.Resource2);
            _logger.LogInformation($"Acessing URL: {requestUrl}");
            var responseHtml = await ExecuteHtmlRequest(requestUrl, false);

            var resultHtml = responseHtml.CssSelect("#sitemap-pane-categoria")
                                            .CssSelect(".sitemap-block")
                                                .FirstOrDefault();

            _logger.LogInformation($"Extracting result elements...");

            resultHtml?.FirstChild
                        .ChildNodes
                            .ToList()
                                .ForEach(c =>
                                {
                                    if (c.LastChild.Name == "ul" && c.LastChild.HasChildNodes)
                                        htmlNodes.AddRange(c.LastChild.ChildNodes);
                                });
            
            _logger.LogInformation($"Number of itens:{htmlNodes.Count}");
            _logger.LogInformation($"Converting resulting elements...");
            
            ConcurrentStack<CategoryDTO> categoriesToAdd = new ConcurrentStack<CategoryDTO>();

            Parallel.ForEach(htmlNodes, element =>
            {
                var categoryElement = element.FirstChild
                                                   .CssSelect("a")
                                                       .FirstOrDefault();

                ConvertHtmlToCategories(categoriesToAdd, categoryElement);
            });

            //foreach (var element in htmlNodes)
            //{
            //    var categoryElement = element.FirstChild
            //                                      .CssSelect("a")
            //                                          .FirstOrDefault();

            //    ConvertHtmlToCategories(categoriesToAdd, categoryElement);
            //}

            _logger.LogInformation($"Number of obtained categories:{categoriesToAdd.Count}");
            return categoriesToAdd.ToList();
        }
        public List<ProductDTO> GetProductsByCategory(Guid categoryId, string categoryUrl)
        {
            List<ProductDTO> products = new List<ProductDTO>();
            List<HtmlNode> resultElements = new List<HtmlNode>();
            bool hasItens = true;
            var remoteAddress = new Uri("http://localhost:4444/wd/hub");
            var timeout = TimeSpan.FromSeconds(180);

            #region Commented Code
            //using (IWebDriver driver = new RemoteWebDriver(remoteAddress, _chromeOptions.ToCapabilities(), timeout))
            //{
            //    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

            //    _logger.LogInformation($"Acessing URL: {categoryUrl}");

            //    driver.Navigate().GoToUrl(categoryUrl);

            //    wait.IgnoreExceptionTypes(exceptionTypes: typeof(NoSuchElementException));
            //    _logger.LogInformation($"Extracting result elements...");

            //    var resultHtml = wait.Until(w => w.FindElement(By.CssSelector("div[class$=main-grid]"))
            //                                        .FindElements(By.CssSelector("div[class^=product-v2__ProductCardV2]")))
            //                                            .Select(element => element.FindElement(By.CssSelector("div[class^=RippleContainer]")))
            //                                                .ToList();
            //    hasItens = resultHtml.Any();

            //    _logger.LogInformation($"Has itens:{hasItens}");
            //    _logger.LogInformation($"Number of itens:{resultHtml.Count}");

            //    Actions actions = new Actions(driver);

            //    foreach (var htmlElement in resultHtml)
            //        actions = actions.MoveToElement(htmlElement);

            //    _logger.LogInformation($"Performing navigation actions...");
            //    actions.Perform();
            //    actions.Perform();
            //    _logger.LogInformation($"Actions performed!");

            //    resultElements.AddRange(resultHtml.Select(r => HtmlNode.CreateNode(r.GetAttribute("innerHTML"))));

            //    _logger.LogInformation($"Number of resulted elements added: {resultElements.Count}");

            //    driver.Quit();
            //    driver.Dispose();
            //}
            #endregion

            _logger.LogInformation($"Executing HTML request on URL: {categoryUrl}");

            var responseTask = ExecuteHtmlRequest(categoryUrl);

            responseTask.Wait();

            try
            {
                _logger.LogInformation($"Extracting result elements...");

                var resultHtml = responseTask.Result.CssSelect("div[class$=main-grid]")
                                        .FirstOrDefault()
                                            .CssSelect("div[class^=product-v2__ProductCardV2]")
                                                .Select(element =>
                                                {
                                                    return element.LastChild.FirstChild;
                                                })
                                                .ToList();

                hasItens = resultHtml.Any();

                _logger.LogInformation($"Has itens:{hasItens}");
                _logger.LogInformation($"Number of itens:{resultHtml.Count}");

                #region Commented Code
                //Actions actions = new Actions(driver);

                //foreach (var htmlElement in resultHtml)
                //    actions = actions.MoveToElement(htmlElement);

                //_logger.LogInformation($"Performing navigation actions...");
                //actions.Perform();
                //actions.Perform();
                //_logger.LogInformation($"Actions performed!");
                #endregion

                _logger.LogInformation($"Converting resulting elements...");

                Parallel.ForEach(resultHtml, element =>
                {
                    var result = ConvertHtmlToProduct(categoryId, element);

                    if (result != null)
                        products.Add(result);
                });

                _logger.LogInformation($"Number of obtained products:{products.Count}");

                #region Test Code
                //foreach (var element in resultElements)
                //{
                //    var result = await ConvertHtmlToProduct(element);

                //    if (result != null)
                //        products.Add(result);
                //}
                #endregion
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"Page has no items!");
                _logger.LogError(ex, $"Error occurred while getting products: {ex.Message}");
            }

            return products;
        }
        public async Task<List<ProductReviewDTO>> GetProductReviews(RequestB2WReviewDTO requestB2WReview)
        {
            ConcurrentStack<ProductReviewDTO> reviewsToAdd = new ConcurrentStack<ProductReviewDTO>();

            string reviewsUrl = _websiteConfiguration.ProductReviewsAddress;

            try
            {
                //Required parameter
                reviewsUrl = string.Concat(reviewsUrl, "?", requestB2WReview.Filter);

                if (!string.IsNullOrEmpty(requestB2WReview.Offset))
                    reviewsUrl = string.Concat(reviewsUrl, "&", requestB2WReview.Offset);

                if (!string.IsNullOrEmpty(requestB2WReview.Sort))
                    reviewsUrl = string.Concat(reviewsUrl, "&", requestB2WReview.Limit);

                if (!string.IsNullOrEmpty(requestB2WReview.Sort))
                    reviewsUrl = string.Concat(reviewsUrl, "&", requestB2WReview.Sort);

                _logger.LogInformation($"Executing Json request on URL: {reviewsUrl}");

                var reviewsResponse = await ExecuteJsonRequest(reviewsUrl);

                var reviews = JsonConvert.DeserializeObject<RequestB2WReviewResultDTO>(reviewsResponse, new JsonSerializerSettings
                {
                    Culture = CultureInfo.GetCultureInfo("pt-BR")
                });

                if (reviews != null)
                {
                    _logger.LogInformation($"Request returned {reviews.Results.Count()} reviews!");

                    Parallel.ForEach(reviews.Results, review =>
                    {
                        ProductReviewDTO productReview = new ProductReviewDTO();

                        productReview.Reviewer = "E-Consumidor";

                        productReview.Date = Convert.ToDateTime(review.SubmissionTime, CultureInfo.GetCultureInfo("pt-BR"));

                        productReview.Title = review.Title;
                        productReview.Text = review.ReviewText;
                        productReview.IsRecommended = review.IsRecommended;
                        productReview.Stars = review.Rating;

                        reviewsToAdd.Push(productReview);
                    });

                    _logger.LogInformation($"A total of {reviewsToAdd.Count()} was added for this product!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting reviews: {ex.Message}");
            }

            return reviewsToAdd.ToList();
        }
        private void ConvertHtmlToCategories(ConcurrentStack<CategoryDTO> categoriesToAdd, HtmlNode categoryElement, string categoryName = null)
        {
            CategoryDTO productCategory = new CategoryDTO();

            if (!string.IsNullOrEmpty(categoryName))
            {
                productCategory.Name = categoryName;
                productCategory.SubType = categoryElement.InnerText.Trim();
            }
            else
                productCategory.Name = categoryElement.InnerText.Trim();

            _logger.LogInformation($"Got category: {productCategory.Name}!");

            productCategory.DataProvider = _dataProvider;

            productCategory.Url = string.Concat(_websiteConfiguration.BaseAdress, categoryElement.GetAttributeValue("href"));

            var responseHtml = ExecuteHtmlRequest(productCategory.Url);

            responseHtml.Wait();

            if (responseHtml.Result.CssSelect("div[class$=main-grid]").Any())
            {
                var productCountElement = responseHtml.Result.CssSelect(".sidebarFooter-product-count")
                                                                .FirstOrDefault();

                if (productCountElement != null)
                {
                    var strProductCount = string.Join("", productCountElement.InnerText.Trim().ToCharArray().Where(Char.IsDigit));

                    productCategory.NumberOfProducts = Convert.ToInt32(strProductCount);

                    _logger.LogInformation($"[{productCategory.Name}] The category has {strProductCount} products!");
                }

                _logger.LogInformation($"[{productCategory.Name}] Adding category to list...");

                categoriesToAdd.Push(productCategory);
            }
            else if(responseHtml.Result.CssSelect("#collapse-categorias").Any())
            {
                var subCategoriesElements = responseHtml.Result.CssSelect("#collapse-categorias")
                                                                .FirstOrDefault()
                                                                .CssSelect("ul[class=filter-list]")
                                                                .FirstOrDefault()
                                                                .CssSelect("li[class=filter-list-item]")
                                                                .Select(element => element.FirstChild)
                                                                .ToList();

                Parallel.ForEach(subCategoriesElements, subCategoryElement =>
                {
                    ConvertHtmlToCategories(categoriesToAdd, subCategoryElement, productCategory.Name);
                });

                //foreach (var subCategoryElement in subCategoriesElements )
                //    ConvertHtmlToCategories(categoriesToAdd, subCategoryElement);
            }
        }
        private ProductDTO ConvertHtmlToProduct(Guid categoryId, HtmlNode htmlNode)
        {
            ProductDTO product = new ProductDTO();

            product.CategoryId = categoryId;
            
            product.DataProvider = _dataProvider;

            var detailUrl = htmlNode.GetAttributeValue("href");

            product.Url = $"{_websiteConfiguration.BaseAdress}{detailUrl}";

            var fillProductSpecsTask = FillProductDetail(product);

            var endIdIndex = detailUrl.IndexOf("?");

            product.ExternalId = detailUrl.Substring(0, endIdIndex)
                                               .Replace("/produto/", "");

            var name = htmlNode.CssSelect("h2[class^=TitleUI]")
                                    .FirstOrDefault();

            if (name != null)
                product.Name = name.InnerText;

            //var image = htmlNode.CssSelect("img[class^=ImageUI]")
            //                        .FirstOrDefault();

            //if (image != null)
            //    product.ImageUrl = image.GetAttributeValue("src");

            fillProductSpecsTask.Wait();

            return product;
        }
        private async Task<ProductDetail> FillProductDetail(ProductDTO product)
        {
            ProductDetail productDetail = new ProductDetail();

            var detailResponse = await ExecuteHtmlRequest(product.Url);

            var image = detailResponse.CssSelect("meta[property='og:image']").FirstOrDefault();

            if (image != null)
                product.ImageUrl = image.GetAttributeValue("content");

            var techSpecs = detailResponse.CssSelect("table[class^=TableUI]")
                                                .CssSelect("tr[class^=Tr]")
                                                    .Where(t => t.ChildNodes.Count == 2)
                                                        .Select(t =>
                                                        {
                                                            var key = t.ChildNodes[0].InnerText?.Trim();
                                                            var value = t.ChildNodes[1].InnerText?.Trim();

                                                            return new KeyValuePair<string, string>(key, value);
                                                        }).ToList();

            //var hasUniqueConstraintInfo = techSpecs.Any(t => t.Key.ToUpper().Contains("CÓDIGO DE BARRAS")
            //                                                        || t.Key.ToUpper().Contains("MODELO"));

            //if (!hasUniqueConstraintInfo)
            //    return null;

            await Task.Run(() =>
            {
                var barCode = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("CÓDIGO DE BARRAS"));
                product.BarCode = barCode.Value;
                techSpecs.Remove(barCode);

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

                var referenceModel = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("REFERÊNCIA DO MODELO"));
                product.ReferenceModel = referenceModel.Value;
                techSpecs.Remove(referenceModel);

                var supplier = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("FORNECEDOR"));
                product.Supplier = supplier.Value;
                techSpecs.Remove(supplier);

                ConcurrentDictionary<string, string> specs = new ConcurrentDictionary<string, string>();

                Parallel.ForEach(techSpecs, spec => specs.TryAdd(spec.Key, spec.Value));

                product.OtherSpecs = JsonConvert.SerializeObject(specs, Newtonsoft.Json.Formatting.None);
            });

            return productDetail;
        }
    }
}

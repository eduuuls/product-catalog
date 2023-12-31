﻿using Firebase.Auth;
using Firebase.Storage;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using ProductCatalog.Domain.Configuration;
using ProductCatalog.Domain.DTO;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.ExternalServices;
using ProductCatalog.Infra.CrossCutting.Extensions;
using ScrapySharp.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ProductCatalog.Infra.Data.ExternalServices.Base
{
    public abstract class B2WExternalService : ExternalService, IB2WExternalService
    {
        private readonly WebsiteConfiguration _websiteConfiguration;
        private readonly ChromeOptions _chromeOptions;
        private readonly DataProvider _dataProvider;
        private readonly FirebaseConfiguration _firebaseConfiguration;
        private readonly FirebaseAuthProvider _firebaseAuthProvider;
        private readonly FirebaseAuthLink _authLink;

        public B2WExternalService(IHttpClientFactory httpClientFactory,
                                    ILogger logger,
                                    ChromeOptions chromeOptions,
                                        IOptions<ExternalServicesConfiguraiton> externalServicesConfiguraiton,
                                            IOptions<FirebaseConfiguration> firebaseConfiguration,
                                                DataProvider dataProvider) : base(httpClientFactory, logger)
        {
            _firebaseConfiguration = firebaseConfiguration.Value;
            _firebaseAuthProvider = new FirebaseAuthProvider(new FirebaseConfig(_firebaseConfiguration.Apikey));
            _dataProvider = dataProvider;
            _chromeOptions = chromeOptions;
            _websiteConfiguration = externalServicesConfiguraiton.Value?
                                            .WebsiteConfigurations?
                                                .FirstOrDefault(w => w.Key == dataProvider.ToString());
            _authLink = _firebaseAuthProvider.SignInWithEmailAndPasswordAsync(_firebaseConfiguration.AuthEmail, _firebaseConfiguration.AuthPassword).Result;
        }
        public async Task<CategoryDTO[]> GetCategoriesData()
        {
            HtmlDocument document = new HtmlDocument();
            List<HtmlNode> htmlNodes = new List<HtmlNode>();
            ConcurrentStack<CategoryDTO> categoriesToAdd = new ConcurrentStack<CategoryDTO>();

            string requestUrl = string.Concat(_websiteConfiguration.BaseAdress,
                                                       "/", _websiteConfiguration.Resource2);

            try
            {
                _logger.LogInformation($"[GetCategoriesData] Acessing URL...");
                var responseHtml = await ExecuteHtmlRequest(requestUrl, false);

                _logger.LogInformation($"[GetCategoriesData] Extracting result elements...");

                htmlNodes = responseHtml.CssSelect("#sitemap-pane-categoria")
                                                .FirstOrDefault()
                                                    .FirstChild
                                                        .CssSelect(".sitemap-list")
                                                            .FirstOrDefault()
                                                                .ChildNodes
                                                                    .ToList();
                
                
                _logger.LogInformation($"[GetCategoriesData] Number of elements: {htmlNodes.Count}");
                _logger.LogInformation($"[GetCategoriesData] Converting resulting elements...");

                Parallel.ForEach(htmlNodes, categoryElement =>
                {
                    CategoryDTO productCategory = new CategoryDTO();
                    List<CategoryLinkDTO> categoryLinks = new List<CategoryLinkDTO>();
                    productCategory.DataProvider = _dataProvider;
                    productCategory.Name = HttpUtility.HtmlDecode(categoryElement.FirstChild.InnerText);
                    
                    _logger.LogInformation($"[ConvertHtmlToCategory] Got category: {productCategory.Name}!");

                    foreach (var categoryLinkElement in categoryElement.LastChild.ChildNodes)
                    {
                        var element = categoryLinkElement.Name == "li" ? categoryLinkElement.FirstChild.CssSelect("a").FirstOrDefault(): categoryLinkElement;

                        var url = HttpUtility.UrlDecode(string.Concat(_websiteConfiguration.BaseAdress, element.GetAttributeValue("href")));

                        categoryLinks.Add(new CategoryLinkDTO() { Url = url, Description = HttpUtility.HtmlDecode(element.InnerText.Trim()) });
                    }

                    productCategory.Links = categoryLinks;
                    
                    categoriesToAdd.Push(productCategory);
                });
                
                _logger.LogInformation($"[GetCategoriesData] Number of obtained categories:{categoriesToAdd.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[GetCategoriesData] Error occurred while getting categories: {ex.Message}");
            }
            finally
            {
                GC.Collect();
            }
            
            return categoriesToAdd.ToArray();
        }
        public async Task<ProductDTO[]> GetProductsData(Guid categoryId, string categoryUrl)
        {
            ConcurrentStack<ProductDTO> products = new ConcurrentStack<ProductDTO>();
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

            _logger.LogInformation($"[GetProductsData] Executing request on category URL...");

            var response = await ExecuteHtmlRequest(categoryUrl, false);

            try
            {
                _logger.LogInformation($"[GetProductsData] Extracting result elements...");

                resultElements = response.CssSelect("div[class$=main-grid]")
                                            .CssSelect("div[class^=product-grid-item]")
                                            .Select(element =>
                                            {
                                                return element.FirstChild.LastChild.FirstChild;
                                            })
                                            .ToList();

                hasItens = resultElements.Any();

                _logger.LogInformation($"[GetProductsData] Has itens:{hasItens}");
                _logger.LogInformation($"[GetProductsData] Number of obtained itens:{resultElements.Count}");

                Parallel.ForEach(resultElements, productElement =>
                {
                    products.Push(ConvertHtmlToProduct(categoryId, productElement));
                });
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"[GetProductsData] Page has no items!");
                _logger.LogError(ex, $"[GetProductsData] Error occurred while getting products: {ex.Message}");
            }
            finally
            {
                GC.Collect();
            }

            return products.ToArray();
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

                _logger.LogInformation($"[GetProductReviews][{requestB2WReview.Filter}] Executing Json request on URL: {reviewsUrl}");

                var reviewsResponse = await ExecuteJsonRequest(reviewsUrl);

                var reviews = JsonConvert.DeserializeObject<RequestB2WReviewResultDTO>(reviewsResponse, new JsonSerializerSettings
                {
                    Culture = CultureInfo.GetCultureInfo("pt-BR")
                });

                if (reviews != null && !reviews.Error)
                {
                    _logger.LogInformation($"[GetProductReviews][{requestB2WReview.Filter}] Request returned {reviews.Results.Count()} reviews!");

                    Parallel.ForEach(reviews.Results, review =>
                    {
                        ProductReviewDTO productReview = new ProductReviewDTO();

                        productReview.Reviewer = "E-Consumidor";

                        productReview.Date = Convert.ToDateTime(review.SubmissionTime, CultureInfo.GetCultureInfo("pt-BR"));

                        productReview.ExternalId = review.Id;
                        productReview.Title = HttpUtility.HtmlDecode(review.Title);
                        productReview.Text = HttpUtility.HtmlDecode(review.ReviewText);
                        productReview.Stars = review.Rating;

                        if(review.IsRecommended.HasValue)
                            productReview.IsRecommended = review.IsRecommended;
                        else if (productReview.Stars.HasValue)
                        {
                            if (productReview.Stars >= 3)
                                productReview.IsRecommended = true;
                            else
                                productReview.IsRecommended = false;
                        }

                        reviewsToAdd.Push(productReview);
                    });

                    _logger.LogInformation($"[GetProductReviews][{requestB2WReview.Filter}] A total of {reviewsToAdd.Count()} was added for this product!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[GetProductReviews][{requestB2WReview.Filter}] Error occurred while getting reviews: {ex.Message}");
            }

            return reviewsToAdd.ToList();
        }
        public CategoryLinkDTO[] GetCategoryAdditionalLinks(CategoryDTO categoryDTO)
        {
            ConcurrentStack<CategoryLinkDTO> resultCategoryLinks = new ConcurrentStack<CategoryLinkDTO>();

            _logger.LogInformation($"[GetCategoryAdditionalLinks][{categoryDTO.Name}] Getting additional links...");

            Parallel.ForEach(categoryDTO.Links, categoryLink =>
            {
                try
                {
                    var responseHtml = ExecuteHtmlRequest(categoryLink.Url).Result;

                    if (responseHtml.CssSelect("div[class$=main-grid]").Any())
                    {
                        var productCountElement = responseHtml.CssSelect(".sidebarFooter-product-count")
                                                                    .FirstOrDefault();

                        if (productCountElement != null)
                        {
                            var strProductCount = string.Join("", productCountElement.InnerText.Trim().ToCharArray().Where(Char.IsDigit));

                            categoryLink.NumberOfProducts = Convert.ToInt32(strProductCount);

                            _logger.LogInformation($"[{categoryLink.Description}] The category link has {strProductCount} products!");
                        }

                        _logger.LogInformation($"[{categoryLink.Description}] Adding category link to list...");

                        resultCategoryLinks.Push(categoryLink);
                    }
                    else if (responseHtml.CssSelect("#collapse-categorias").Any())
                    {
                        var subCategoriesElements = responseHtml.CssSelect("#collapse-categorias")
                                                                .FirstOrDefault()
                                                                .CssSelect("ul[class=filter-list]")
                                                                .FirstOrDefault()
                                                                .CssSelect("li[class=filter-list-item]")
                                                                .Select(element => element.FirstChild)
                                                                .ToList();


                        var links = subCategoriesElements.Select(sub => ConvertHtmlToCategoryLink(sub, categoryDTO.Name));

                        if (!links.Any(link => link.Description == categoryLink.Description))
                        {
                            var categoryLinks = GetCategoryAdditionalLinks(new CategoryDTO() { Name = categoryDTO.Name, Links = links });

                            if (categoryLinks.Any())
                                resultCategoryLinks.PushRange(categoryLinks);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[GetCategoryAdditionalLinks] Error occurred while getting additional links.");
                }
                finally
                {
                    GC.Collect();
                }
            });

            if (!resultCategoryLinks.Any())
                _logger.LogInformation($"[GetCategoryAdditionalLinks] Category has no links: {JsonConvert.SerializeObject(categoryDTO, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");

            return resultCategoryLinks.ToArray();
        }
        public async Task<ProductDTO> GetProductAdditionalData(Guid categoryId, ProductDTO productDTO)
        {
            ProductDetail productDetail = new ProductDetail();

            try
            {
                _logger.LogInformation($"[GetProductAdditionalData][{productDTO.ExternalId} - {productDTO.Name}] Getting additional data...");

                var detailResponse = await ExecuteHtmlRequest(productDTO.Url);

                var techSpecs = detailResponse.CssSelect("table[class^=TableUI]")
                                                    .CssSelect("tr[class^=Tr]")
                                                        .Where(t => t.ChildNodes.Count == 2)
                                                            .Select(t =>
                                                            {
                                                                var key = t.ChildNodes[0].InnerText?.Trim();
                                                                var value = t.ChildNodes[1].InnerText?.Trim();

                                                                return new KeyValuePair<string, string>(key, value);
                                                            }).ToList();

                var hasUniqueConstraintInfo = techSpecs.Any(t => t.Key.ToUpper().Contains("FABRICANTE") || t.Key.ToUpper().Contains("MARCA")) && 
                                                techSpecs.Any(t => t.Key.ToUpper().Contains("MODELO")|| t.Key.ToUpper().Contains("REFERÊNCIA DO MODELO"));

                if (!hasUniqueConstraintInfo)
                {
                    _logger.LogInformation($"[GetProductAdditionalData] Product does not meet the minimum requirements.");
                    _logger.LogInformation(JsonConvert.SerializeObject(techSpecs));
                    return null;
                }

                var image = detailResponse.CssSelect("meta[property='og:image']").FirstOrDefault();

                if (image != null)
                {
                    var imageUrl = image.GetAttributeValue("content");

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        _logger.LogInformation($"[Handle] Processing product image URL...");
                        productDTO.ImageUrl = await UploadImageToFirebase(productDTO, imageUrl);
                        _logger.LogInformation($"[Handle] Image URL process succeed...");
                    }
                }
                else
                    return null;

                await Task.Run(() =>
                {
                    var barCode = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("CÓDIGO DE BARRAS"));
                    productDTO.BarCode = barCode.Value;
                    techSpecs.Remove(barCode);

                    var model = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("MODELO"));
                    productDTO.Model = HttpUtility.HtmlDecode(model.Value);
                    productDTO.Model = !string.IsNullOrEmpty(productDTO.Model) ? productDTO.Model.Trim() : productDTO.Model;
                    techSpecs.Remove(model);

                    var brand = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("MARCA"));
                    productDTO.Brand = HttpUtility.HtmlDecode(brand.Value);
                    productDTO.Brand = !string.IsNullOrEmpty(productDTO.Brand) ? productDTO.Brand.Trim() : productDTO.Brand;
                    techSpecs.Remove(brand);

                    var code = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("CÓDIGO"));
                    productDTO.Code = code.Value;
                    techSpecs.Remove(code);

                    var manufacturer = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("FABRICANTE"));
                    productDTO.Manufacturer = HttpUtility.HtmlDecode(manufacturer.Value);
                    productDTO.Manufacturer = !string.IsNullOrEmpty(productDTO.Manufacturer) ? productDTO.Manufacturer.Trim() : productDTO.Manufacturer;
                    techSpecs.Remove(manufacturer);

                    var referenceModel = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("REFERÊNCIA DO MODELO"));
                    productDTO.ReferenceModel = HttpUtility.HtmlDecode(referenceModel.Value);
                    productDTO.ReferenceModel = !string.IsNullOrEmpty(productDTO.ReferenceModel) ? productDTO.ReferenceModel.Trim(): productDTO.ReferenceModel;
                    techSpecs.Remove(referenceModel);

                    var supplier = techSpecs.FirstOrDefault(t => t.Key.ToUpper().Equals("FORNECEDOR"));
                    productDTO.Supplier = HttpUtility.HtmlDecode(supplier.Value);
                    productDTO.Supplier = !string.IsNullOrEmpty(productDTO.Supplier) ? productDTO.Supplier.Trim() : productDTO.Supplier;
                    techSpecs.Remove(supplier);

                    ConcurrentDictionary<string, string> specs = new ConcurrentDictionary<string, string>();

                    Parallel.ForEach(techSpecs, spec => specs.TryAdd(spec.Key, spec.Value));

                    productDTO.OtherSpecs = JsonConvert.SerializeObject(specs, Newtonsoft.Json.Formatting.None);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[GetProductAdditionalData][{productDTO.ExternalId} - {productDTO.Name}] Error occurred while getting additional data for product.");
            }
            finally
            {
                GC.Collect();
            }

            return productDTO;
        }
        private CategoryLinkDTO ConvertHtmlToCategoryLink(HtmlNode categoryElement, string categoryName = null)
        {
            var url = categoryElement.GetAttributeValue("href");

            url = url.Substring(0, url.IndexOf("?"));

            return new CategoryLinkDTO()
            {
                Url = string.Concat(_websiteConfiguration.BaseAdress, url),
                Description = HttpUtility.HtmlDecode(categoryElement.InnerText.Trim())
            };
        }
        private ProductDTO ConvertHtmlToProduct(Guid categoryId, HtmlNode productElement)
        {
            ProductDTO product = new ProductDTO();

            product.CategoryId = categoryId;

            product.DataProvider = _dataProvider;

            var detailUrl = productElement.GetAttributeValue("href");

            product.Url = HttpUtility.UrlDecode($"{_websiteConfiguration.BaseAdress}{detailUrl}");

            var endIdIndex = detailUrl.IndexOf("?");

            product.ExternalId = detailUrl.Substring(0, endIdIndex)
                                               .Replace("/produto/", "");

            var name = productElement.CssSelect("h2[class^=TitleUI]")
                                        .FirstOrDefault();

            if (name != null)
                product.Name = HttpUtility.HtmlDecode(name.InnerText);

            _logger.LogInformation($"[ConvertHtmlToProduct] Got product: {product.Name}!");

            return product;
        }
        private async Task<string> UploadImageToFirebase(ProductDTO product, string imageUrl)
        {
            FirebaseStorageOptions firebaseStorageOptions = new FirebaseStorageOptions();
            var cancelationToken = new CancellationTokenSource();
            string resultUrl = string.Empty;

            _logger.LogInformation($"[Handle] Acessing image URL from external data provider...");

            var imageStream = await ExecuteImageRequest(imageUrl);

            if (imageStream != null)
            {
                _logger.LogInformation($"[Handle] Got image stream!");
                _logger.LogInformation($"[Handle] Authenticating on Firebase...");

                if (_authLink.IsExpired())
                {
                    _logger.LogInformation($"[Handle] Firebase token expired. Getting fresh token...");
                    var authLink = await _authLink.GetFreshAuthAsync();
                    firebaseStorageOptions.AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken);
                }
                else
                    firebaseStorageOptions.AuthTokenAsyncFactory = () => Task.FromResult(_authLink.FirebaseToken);

                firebaseStorageOptions.ThrowOnCancel = true;

                _logger.LogInformation($"[Handle] Authentication was successful!");

                var storage = new FirebaseStorage(_firebaseConfiguration.StorageBucketUrl, firebaseStorageOptions);

                var imageExtension = imageUrl.Split(".").Last();

                _logger.LogInformation($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Requesting image from URL...");

                resultUrl = await storage.Child("images")
                                            .Child("products")
                                                .Child($"Category-{product.CategoryId}")
                                                    .Child($"{product.ExternalId}.{imageExtension}").PutAsync(imageStream);

                _logger.LogInformation($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Image uploaded to Firebase successfully!");
                _logger.LogInformation($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Firebase image URL: {resultUrl}.");
            }
            else
            {
                _logger.LogInformation($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Image not found at URL: {imageUrl}.");
            }

            return resultUrl;
        }
    }
}

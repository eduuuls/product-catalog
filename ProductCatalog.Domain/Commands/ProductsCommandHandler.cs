using Firebase.Auth;
using Firebase.Storage;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Configuration;
using ProductCatalog.Domain.DTO.Query;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Extensions;
using ProductCatalog.Domain.Interfaces.Queries;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Infra.CrossCutting.Resilience;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ProductCatalog.Domain.Commands
{
    public class ProductsCommandHandler : CommandHandler, 
                                            IRequestHandler<CreateNewProductsCommand, ValidationResult>,
                                            IRequestHandler<CreateNewProductCommand, ValidationResult>,
                                            IRequestHandler<MergeProductCommand, ValidationResult>
    {
        private readonly IProductsQuery _productsQuery;
        private readonly IProductsRepository _productsRepository;
        private readonly IProductsDetailRepository _productsDetailRepository;
        private readonly IProductsReviewsRepository _productsReviewsRepository;
        private readonly FirebaseConfiguration _firebaseConfiguration;
        private readonly FirebaseAuthProvider _firebaseAuthProvider;
        private readonly FirebaseAuthLink _authLink;
        private readonly HttpClient _httpClient;

        public ProductsCommandHandler(ILogger<ProductsCommandHandler> logger, 
                                        IProductsRepository productsRepository, IProductsDetailRepository productsDetailRepository,
                                            IProductsReviewsRepository productsReviewsRepository, IProductsQuery productsQuery,
                                                IOptions<FirebaseConfiguration> firebaseConfiguration,
                                                    IHttpClientFactory clientFactory)
            : base(logger)
        {
            _productsRepository = productsRepository;
            _productsDetailRepository = productsDetailRepository;
            _productsReviewsRepository = productsReviewsRepository;
            _productsQuery = productsQuery;
            _firebaseConfiguration = firebaseConfiguration.Value;
            _firebaseAuthProvider = new FirebaseAuthProvider(new FirebaseConfig(_firebaseConfiguration.Apikey));
            _httpClient = clientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(3);
            _authLink = _firebaseAuthProvider.SignInWithEmailAndPasswordAsync(_firebaseConfiguration.AuthEmail, _firebaseConfiguration.AuthPassword).Result;
        }

        public async Task<ValidationResult> Handle(CreateNewProductsCommand message, CancellationToken cancellationToken)
        {
            LogInfo($"[Handle] Starting handling products creation...");

            message.Commands.ForEach(c =>
            {
                LogInfo($"[Handle] Handling product: {c.Name}");

                LogInfo($"[Handle] Validating product...");
                
                if (!c.IsValid())
                {
                    LogInfo($"[Handle] Product didn't pass validation process...");
                    return;
                }
                
                LogInfo($"[Handle] Product validation Ok...");

                var product = new Product(Guid.NewGuid(), c.CategoryId, c.ExternalId, c.Name, c.Description, c.BarCode,
                                            c.Code, c.Brand, c.Manufacturer, c.Model, c.ReferenceModel, c.Supplier, c.OtherSpecs,
                                                c.Url, c.ImageUrl, c.DataProvider, c.Reviews);

                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    LogInfo($"[Handle] Processing product image URL...");
                    product.ImageUrl = UploadImageToFirebase(product, product.ImageUrl).Result;
                    LogInfo($"[Handle] Image URL process succeed...");
                }

                var existingProduct = _productsRepository.GetByKey(product.CategoryId, product.ExternalId, product.DataProvider).Result;

                if (existingProduct != null)
                {
                    LogInfo($"[Handle] Product already exists. Executing product update...");

                    existingProduct.Description = product.Description;
                    existingProduct.Name = product.Name;
                    existingProduct.Url = product.Url;
                    existingProduct.ImageUrl = product.ImageUrl;

                    var existingProductDetail = _productsDetailRepository.GetByProductId(existingProduct.Id).Result;

                    existingProductDetail.Manufacturer = product.Detail.Manufacturer;
                    existingProductDetail.Model = product.Detail.Model;
                    existingProductDetail.BarCode = product.Detail.BarCode;
                    existingProductDetail.Brand = product.Detail.Brand;
                    existingProductDetail.Code = product.Detail.Code;
                    existingProductDetail.ReferenceModel = product.Detail.ReferenceModel;
                    existingProductDetail.Supplier = product.Detail.Supplier;
                    existingProductDetail.OtherSpecs = product.Detail.OtherSpecs;

                    product.AddDomainEvent(new ProductCreatedUpdatedEvent(existingProduct.Id, existingProduct.CategoryId, existingProduct.ExternalId, existingProduct.Name,
                                                                            existingProduct.Description, existingProduct.Url, existingProduct.ImageUrl, existingProduct.DataProvider,
                                                                                existingProduct.Detail, existingProduct.Reviews));
                }
                else
                {
                    LogInfo($"[Handle] Creating new product...");

                    product.AddDomainEvent(new ProductCreatedUpdatedEvent(product.Id, product.CategoryId, product.ExternalId, product.Name,
                                                                    product.Description, product.Url, product.ImageUrl, product.DataProvider,
                                                                        product.Detail, product.Reviews));

                    _productsRepository.Add(product);
                }
            });

            LogInfo($"[Handle] Commiting process...");
            return await Commit(_productsRepository.UnitOfWork);
        }

        public async Task<ValidationResult> Handle(CreateNewProductCommand message, CancellationToken cancellationToken)
        {
            LogInfo($"[Handle] Starting handling product creation...");
            LogInfo($"[Handle] Validating product...");

            if (!message.IsValid())
            {
                LogInfo($"[Handle] Product didn't pass validation process.");
                LogInfo($"[Handle] Product was not created.");
                return message.ValidationResult;
            };
            
            LogInfo($"[Handle] Product validation Ok...");

            var product = new Product(Guid.NewGuid(), message.CategoryId, message.ExternalId, message.Name, message.Description, message.BarCode,
                                        message.Code, message.Brand, message.Manufacturer, message.Model, message.ReferenceModel, message.Supplier, 
                                            message.OtherSpecs, message.Url, message.ImageUrl, message.DataProvider, message.Reviews);

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                LogInfo($"[Handle] Processing product image URL...");
                product.ImageUrl = await UploadImageToFirebase(product, product.ImageUrl);
                LogInfo($"[Handle] Image URL process succeed...");
            }


            var existingProduct = await _productsRepository.GetByKey(product.CategoryId, product.ExternalId, product.DataProvider);

            if (existingProduct != null)
            {
                LogInfo($"[Handle] Product already exists. Executing product update...");

                existingProduct.Description = product.Description;
                existingProduct.Name = product.Name;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.Url = product.Url;
                existingProduct.RelevancePoints = product.RelevancePoints;

                var existingProductDetail = await _productsDetailRepository.GetByProductId(existingProduct.Id);

                existingProductDetail.Manufacturer = product.Detail.Manufacturer;
                existingProductDetail.Model = product.Detail.Model;
                existingProductDetail.BarCode = product.Detail.BarCode;
                existingProductDetail.Brand = product.Detail.Brand;
                existingProductDetail.Code = product.Detail.Code;
                existingProductDetail.ReferenceModel = product.Detail.ReferenceModel;
                existingProductDetail.Supplier = product.Detail.Supplier;
                existingProductDetail.OtherSpecs = product.Detail.OtherSpecs;

                existingProductDetail.AddDomainEvent(new ProductCreatedUpdatedEvent(existingProduct.Id, existingProduct.CategoryId, existingProduct.ExternalId, 
                                                            existingProduct.Name, existingProduct.Description, existingProduct.Url, existingProduct.ImageUrl, 
                                                                existingProduct.DataProvider, existingProduct.Detail, existingProduct.Reviews));
            }
            else
            {
                LogInfo($"[Handle] Creating new product...");

                product.AddDomainEvent(new ProductCreatedUpdatedEvent(product.Id, product.CategoryId, product.ExternalId, product.Name,
                                                                product.Description, product.Url, product.ImageUrl, product.DataProvider,
                                                                    product.Detail, product.Reviews));

                _productsRepository.Add(product);
            }

            LogInfo($"[Handle] Commiting process...");

            return await Commit(_productsRepository.UnitOfWork);
        }

        public async Task<ValidationResult> Handle(MergeProductCommand message, CancellationToken cancellationToken)
        {
            LogInfo($"[Handle] Starting handling product merge...");
            LogInfo($"[Handle] Validating merge parameters...");

            if (!message.IsValid())
            {
                LogInfo($"[Handle] Merge parameters didn't pass validation process.");
                LogInfo($"[Handle] Product was not merged.");
                return message.ValidationResult;
            }
            
            LogInfo($"[Handle] Merge parameters validation Ok...");

            var product = new Product(message.Id, message.CategoryId, message.ExternalId, message.Name, message.Description, message.BarCode,
                                        message.Code, message.Brand, message.Manufacturer, message.Model, message.ReferenceModel, message.Supplier,
                                            message.OtherSpecs, message.Url, message.ImageUrl, message.DataProvider, message.Reviews);

            LogInfo($"[Handle] Executing merge for product: {product.Name}");

            if (!string.IsNullOrEmpty(product.Detail.Model))
            {
                LogInfo($"[Handle] Executing merge by Model: {product.Detail.Model}");
                await MergeProducts("Model", product.Detail.Model);
            }
            else if (!string.IsNullOrEmpty(product.Detail.ReferenceModel))
            {
                LogInfo($"[Handle] Executing merge by ReferenceModel: {product.Detail.ReferenceModel}");
                await MergeProducts("ReferenceModel", product.Detail.ReferenceModel);
            }
            else if (!string.IsNullOrEmpty(product.Detail.BarCode))
            {
                LogInfo($"[Handle] Executing merge by BarCode: {product.Detail.BarCode}");
                await MergeProducts("BarCode", product.Detail.BarCode);
            }

            LogInfo($"[Handle] Commiting merge process...");
            return await Commit(_productsRepository.UnitOfWork);
        }

        private async Task MergeProducts(string mergeKey, string mergeValue)
        {
            string mergedProductsId = string.Empty;
            List<ProductReview> reviewsToTransfer = new List<ProductReview>();

            var products = await _productsQuery.GetProductByParameter(mergeKey, mergeValue);

            if (products.Count() == 1)
            {
                LogInfo($"[Handle] Not enough products to merge. Only one product was found.");
                return;
            }

            LogInfo($"[Handle] {products.Count()} identical products to merge!");

            var maxRelevancePoints = products.Max(p => p.RelevancePoints);
            var mainProduct = products.First(p => p.RelevancePoints == maxRelevancePoints);
            var productsToMerge = products.Where(p => p.Id != mainProduct.Id);

            foreach (var mergingProduct in productsToMerge)
            {
                LogInfo($"[Handle] Merging product {mergingProduct.Id} into main product {mainProduct.Id}");

                mainProduct = mainProduct.MergeProducts(mergingProduct);

                mergedProductsId = mergedProductsId.Equals(string.Empty) ?
                                            mergingProduct.Id.ToString() :
                                                string.Join(",", mergedProductsId, mergingProduct.Id);

                if (mergingProduct.Reviews.Any())
                    reviewsToTransfer.AddRange(await _productsReviewsRepository.GetByProductId(mergingProduct.Id));
            }

            var productDetail = await _productsDetailRepository.GetByProductId(mainProduct.Id);
            var product = await _productsRepository.GetById(mainProduct.Id);

            reviewsToTransfer.ForEach(r => r.ProductId = mainProduct.Id);
            productDetail.BarCode = mainProduct.Detail.BarCode;
            productDetail.Brand = mainProduct.Detail.Brand;
            productDetail.Manufacturer = mainProduct.Detail.Manufacturer;
            productDetail.ReferenceModel = mainProduct.Detail.ReferenceModel;
            productDetail.Supplier = mainProduct.Detail.Supplier;
            productDetail.OtherSpecs = mainProduct.Detail.OtherSpecs;
            productDetail.MergedProductsId = mergedProductsId;

            LogInfo($"[Handle] Old product revelance points: {product.RelevancePoints}");
            product.RelevancePoints = productDetail.GetRelevancePoints();
            LogInfo($"[Handle] Current product revelance points: {product.RelevancePoints}");

            product.AddDomainEvent(new ProductsMergeDoneEvent(product.Id));

            LogInfo($"[Handle] Merge done!");
        }

        private async Task<string> UploadImageToFirebase(Product product, string imageUrl)
        {
            FirebaseStorageOptions firebaseStorageOptions = new FirebaseStorageOptions();
            var cancelationToken = new CancellationTokenSource();
            string resultUrl = string.Empty;

            LogInfo($"[Handle] Acessing image URL from external data provider...");

            var imageStream = await ExecuteImageRequest(imageUrl);

            if (imageStream != null)
            {
                LogInfo($"[Handle] Got image stream!");
                LogInfo($"[Handle] Authenticating on Firebase...");

                if (_authLink.IsExpired())
                {
                    LogInfo($"[Handle] Firebase token expired. Getting fresh token...");
                    var authLink = await _authLink.GetFreshAuthAsync();
                    firebaseStorageOptions.AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken);
                }
                else
                    firebaseStorageOptions.AuthTokenAsyncFactory = () => Task.FromResult(_authLink.FirebaseToken);

                firebaseStorageOptions.ThrowOnCancel = true;

                LogInfo($"[Handle] Authentication was successful!");

                var storage = new FirebaseStorage(_firebaseConfiguration.StorageBucketUrl, firebaseStorageOptions);

                var imageExtension = imageUrl.Split(".").Last();

                LogInfo($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Requesting image from URL...");

                resultUrl = await storage.Child("images")
                                            .Child("products")
                                                .Child($"Category-{product.CategoryId}")
                                                    .Child($"{product.ExternalId}.{imageExtension}").PutAsync(imageStream);

                LogInfo($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Image uploaded to Firebase successfully!");
                LogInfo($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Firebase image URL: {resultUrl}.");
            }
            else
            {
                LogInfo($"[UploadImageToFirebase][{product.ExternalId} - {product.Name}] Image not found at URL: {imageUrl}.");
            }

            return resultUrl;
        }
        public async Task<Stream> ExecuteImageRequest(string requestUrl)
        {
            var response = await RetryPolices.GetRetryPolicy().ExecuteAsync(() =>
            {
                return _httpClient.GetAsync(requestUrl);
            });

            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            return await response.Content.ReadAsStreamAsync();
        }
    }
}

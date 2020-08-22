using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Polly;
using Polly.Retry;
using ProductCatalog.Domain.Interfaces.ExternalServices.Base;
using ProductCatalog.Infra.CrossCutting.Resilience;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.ExternalServices.Base
{
    public abstract class ExternalService: IExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly Random _random;
        protected readonly ILogger _logger;

        public ExternalService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(3);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36 Edg/84.0.522.40");
            _random = new Random();
        }

        public ExternalService(IHttpClientFactory clientFactory, ILogger logger)
        {
            _httpClient = clientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(3);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36 Edg/84.0.522.40");
            _logger = logger;
            _random = new Random();
        }

        public async Task<HtmlNode> ExecuteHtmlRequest(string requestUrl, bool takeBreathTime = true)
        {
            HtmlDocument document = new HtmlDocument();

            if (takeBreathTime)
            {
                var breathTime = GenerateBeathTime();

                _logger.LogInformation($"[ExecuteHtmlRequest] BreathTime: {breathTime} Milliseconds");

                Thread.Sleep(breathTime);
            }

            var response = await RetryPolices.GetRetryPolicy().ExecuteAsync(() =>
            {
                return _httpClient.GetAsync(requestUrl);
            });

            var content = await response.Content.ReadAsStringAsync();

            document.LoadHtml(content);

            return document.DocumentNode;
        }

        public async Task<Stream> ExecuteImageRequest(string requestUrl)
        {
            var response = await RetryPolices.GetRetryPolicy().ExecuteAsync(() =>
            {
                return _httpClient.GetAsync(requestUrl);
            });

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            return await response.Content.ReadAsStreamAsync();
        }

        public HtmlNode ExecuteWebDriverRequest(string requestUrl)
        {
            HtmlDocument document = new HtmlDocument();

            Proxy proxy = new Proxy();
            proxy.IsAutoDetect = false;
            proxy.Kind = ProxyKind.Manual;
            proxy.HttpProxy = "http://proxy-server.scraperapi.com:8001";

            var chromeOpts = new ChromeOptions();

            chromeOpts.AddArguments(new List<string>()
            {
                //"--proxy-server=http://scraperapi:5467bfa9b99839c256739a6f6ef409ad@proxy-server.scraperapi.com:8001",
                "ignore-certificate-errors",
                "--silent-launch",
                "no-sandbox",
                //"--disable-gpu",
                "--headless",
                "--disable-extensions"
            });

            //using (IWebDriver driver = new RemoteWebDriver(new Uri("https://D2N0tPrLbua93AHmhZc8ORLM9i0IwUE0:NiQ7Y7IZsJYlawxhmAf1hRDgZuPmfkcT@4etrr57g-hub.gridlastic.com/wd/hub"), chromeOpts))
            using (IWebDriver driver = new ChromeDriver())
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                driver.Navigate().GoToUrl($"http://api.scraperapi.com/?api_key=5467bfa9b99839c256739a6f6ef409ad&url={requestUrl}");
                //driver.Navigate().GoToUrl(requestUrl);

                wait.IgnoreExceptionTypes(exceptionTypes: typeof(NoSuchElementException));

                IWebElement bottomElement = wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".ft-address")));
                IWebElement topElement = driver.FindElement(By.Id("section-top"));

                Actions actions = new Actions(driver);

                actions.MoveToElement(bottomElement).Perform();
                Thread.Sleep(200);
                actions.MoveToElement(topElement).Perform();
                Thread.Sleep(200);
                actions.MoveToElement(bottomElement).Perform();
                Thread.Sleep(200);
                actions.MoveToElement(topElement).Perform();
                Thread.Sleep(200);
                actions.MoveToElement(bottomElement).Perform();
                Thread.Sleep(200);
                actions.MoveToElement(topElement).Perform();

                var html = driver.FindElements(By.CssSelector("html"));

                if (html != null && html.Any())
                    document.LoadHtml(html.FirstOrDefault().GetAttribute("innerHTML"));

                driver.Quit();
            }

            return document.DocumentNode;
        }

        public async Task<string> ExecuteJsonRequest(string requestUrl, bool takeBreathTime = true)
        {
            if (takeBreathTime)
            {
                var breathTime = GenerateBeathTime();

                _logger.LogInformation($"[ExecuteHtmlRequest] BreathTime: {breathTime} Milliseconds");

                Thread.Sleep(breathTime);
            }

            var response = await RetryPolices.GetRetryPolicy().ExecuteAsync(() =>
            {
                return _httpClient.GetAsync(requestUrl);
            });

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<HttpResponseMessage> ExecuteHttpRequest(Uri requestUri, string referer)
        {
            _httpClient.DefaultRequestHeaders.Add("Referer", referer);
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

            var response = await RetryPolices.GetRetryPolicy().ExecuteAsync(() =>
            {
                return _httpClient.GetAsync(requestUri);
            });

            return response;
        }

        private int GenerateBeathTime()
        {
            var minValue = _random.Next(1000, 60000);
            var maxValue = _random.Next(61000, 120000);

            return _random.Next(minValue, maxValue);
        }
    }
}

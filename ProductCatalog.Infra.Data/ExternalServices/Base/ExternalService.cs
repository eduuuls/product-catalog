using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Polly;
using Polly.Retry;
using ProductCatalog.Domain.Interfaces.ExternalServices.Base;
using System;
using System.Collections.Generic;
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
    public abstract class ExternalService: IExternalService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly Random _random;
        private readonly int _minBreathTime;
        private readonly int _maxBreathTime;
        protected readonly ILogger _logger;

        public ExternalService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _random = new Random();
            _minBreathTime = 15000;
            _maxBreathTime = 180000;
        }

        public ExternalService(IHttpClientFactory clientFactory, ILogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _random = new Random();
            _minBreathTime = 15000;
            _maxBreathTime = 180000;
        }
        
        public async Task<HtmlNode> ExecuteHtmlRequest(string requestUrl, bool takeBreathTime = true)
        {
            HtmlDocument document = new HtmlDocument();

            if (takeBreathTime)
            {
                var breathTime = _random.Next(_minBreathTime, _maxBreathTime);

                _logger.LogInformation($"[ExecuteHtmlRequest] BreathTime: {breathTime} Milliseconds");

                Thread.Sleep(breathTime);
            }

            using (var httpClient = _clientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(1);

                var response = GetRetryPolicy().Execute(() =>
                {
                   return httpClient.GetAsync(requestUrl).Result;
                });

                var content = await response.Content.ReadAsStringAsync();

                document.LoadHtml(content);
            }

            return document.DocumentNode;
        }

        public async Task<Stream> ExecuteImageRequest(string requestUrl)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(1);

                var response = GetRetryPolicy().Execute(() =>
                {
                    return httpClient.GetAsync(requestUrl).Result;
                });

                return await response.Content.ReadAsStreamAsync();
            }
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
                var breathTime = _random.Next(_minBreathTime, _maxBreathTime);

                _logger.LogInformation($"[ExecuteHtmlRequest] BreathTime: {breathTime} Milliseconds");

                Thread.Sleep(breathTime);
            }

            using (var httpClient = _clientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(1);

                var response = GetRetryPolicy().Execute(() =>
                {
                    return httpClient.GetAsync(requestUrl).Result;
                });

                return await response.Content.ReadAsStringAsync();
            }
        }

        public HttpResponseMessage ExecuteHttpRequest(Uri requestUri, string referer)
        {
            using (var httpClient = _clientFactory.CreateClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(1);
                httpClient.DefaultRequestHeaders.Add("Referer", referer);
                httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");

                var response = GetRetryPolicy().Execute(() =>
                {
                    return httpClient.GetAsync(requestUri).Result;
                });
                
                return response;
            }
        }

        private RetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(message => message.StatusCode != HttpStatusCode.OK)
                            .WaitAndRetry(new[]
                              {
                                TimeSpan.FromSeconds(5),
                                TimeSpan.FromSeconds(15),
                                TimeSpan.FromSeconds(30),
                                TimeSpan.FromSeconds(45),
                                TimeSpan.FromSeconds(60)
                              });
        }
    }
}

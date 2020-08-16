using OpenQA.Selenium;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using ProductCatalog.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace ProductCatalog.Infra.CrossCutting
{
    public class SeleniumHelper
    {
        public List<Producot> SearchProduct(string searchText)
        {
            List<Producot> products = new List<Producot>();
            
            ChromeOptions browserOptions = new ChromeOptions();
            browserOptions.AddArgument("--headless");
            browserOptions.AddArgument("--disable-gpu");

            using (IWebDriver driver = new RemoteWebDriver(new Uri("http://40.119.50.194:4444/wd/hub"), browserOptions))
            //using (IWebDriver driver = new ChromeDriver(browserOptions))
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                driver.Navigate().GoToUrl("https://www.buscape.com.br/");

                wait.Until(w =>
                {
                    //try
                    //{
                    try
                    {
                        w.FindElement(By.CssSelector("div[class=search-bar]"))
                            .FindElement(By.CssSelector("input[type=search]"))
                                .SendKeys(searchText + Keys.Enter);
                    }
                    catch

                    {
                        w.FindElement(By.CssSelector(".zsearch__fake"))
                            .FindElement(By.TagName("input"))
                                .Click();

                        w.FindElement(By.CssSelector(".zsearch__inputGroup"))
                            .FindElement(By.CssSelector("input[data-search=field]"))
                                .SendKeys(searchText + Keys.Enter);
                    }

                    return true;
                });

                var resultElements = wait.Until(w =>
                {
                    return w.FindElement(By.Id("pageSearchResultsBody"))
                                .FindElement(By.CssSelector("div[class=SearchPage_searchList__1rdGp]"))
                                    .FindElements(By.CssSelector(".card"));
                });

                Parallel.ForEach(resultElements, element =>
                {
                    var product = new Producot();

                    //Action setId = () =>
                    //{
                    //    product.Id = element.GetAttribute("data-id");
                    //};

                    //Action setName = () =>
                    //{

                    //    product.Name = element.FindElement(By.CssSelector(".cardBody"))
                    //                            .FindElement(By.CssSelector(".name")).Text;

                    //};

                    //Action setImage = () =>
                    //{

                    //    product.ImageUrl = element.FindElement(By.CssSelector(".image"))
                    //                                .GetAttribute("src");

                    //};

                    //Parallel.Invoke(setId, setName, setImage);

                    product.Id = element.GetAttribute("data-id");

                    product.Name = element.FindElement(By.CssSelector(".cardBody"))
                                                .FindElement(By.CssSelector(".name")).Text;

                    product.ImageUrl = element.FindElement(By.CssSelector(".image"))
                                                    .GetAttribute("src");

                    products.Add(product);
                });

                //foreach (var element in resultElements)
                //{
                //    var product = new ProductCatalogItem();

                //    product.Id = element.GetAttribute("data-id");

                //    product.Name = element.FindElement(By.CssSelector(".cardBody"))
                //                                .FindElement(By.CssSelector(".name")).Text;

                //    product.ImageUrl = element.FindElement(By.CssSelector(".image"))
                //                                    .GetAttribute("src");

                //    products.Add(product);
                //}

                return products;
            }
        }
    }
}

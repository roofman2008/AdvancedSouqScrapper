using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SouqScrapper.ApiModels;
using SouqScrapper.Core;
using SouqScrapper.Helpers;
using SouqScrapper.Models;

namespace SouqScrapper.Parsers
{
    public class CategoryProductsParser : IParser
    {
        public void Process(Website website)
        {
            Console.WriteLine($"{nameof(CategoryProductsParser)} Start");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var categories = website.Categories.Shuffle().ToList();

            //foreach(var category in categories)
            Parallel.ForEach(categories /*.Where(l => l.Url.Contains("/home-decor/"))*/ /*.Take(1)*/,
                new ParallelOptions(){MaxDegreeOfParallelism =  50}, (category) =>
                {
                    try
                    {
                        Console.WriteLine(category.Name);
                        category.Products = new List<Product>();
                        string categoryId = category.Url.ExtractCategoryId();

                        var gridNativeObjs = SouqApi.GetProductsFromCategory(categoryId);

                        if (gridNativeObjs == null)
                        {
                            category.IsFaulty = true;
                            //Debugger.Break();
                            //continue;
                            return;
                        }

                        category.ExpectedProductCount =
                            gridNativeObjs.Count() * SouqApiConstants.ProductSectionListLimit;

                        foreach (var gridObj in gridNativeObjs)
                        {
                            var document = new HtmlDocument();
                            document.LoadHtml(gridObj.body);

                            var productsNodes = document.DocumentNode.Descendants()
                                .FindByNameNAttribute("div", "data-category-name");

                            foreach (var productNode in productsNodes)
                            {
                                var quickViewNode = productNode.Descendants()
                                    .SingleByNameNContainClass("a", new[] {"sPrimaryLink", "img-link"});

                                Product product = new Product()
                                {
                                    Id = Guid.NewGuid(),
                                    Url = quickViewNode.Attributes["href"].Value
                                };

                                if (category.Products.All(l => l.Url != product.Url))
                                {
                                    product.Category = category;
                                    category.Products.Add(product);
                                }
                            }
                        }

                        category.ActualProductCount = category.Products.Count;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(category.Name);
                        Console.WriteLine(e);
                        Debugger.Break();
                    }
                });

            website.Categories = categories.Where(l => !l.IsFaulty).ToList();
            sw.Stop();
            Console.WriteLine($"{nameof(CategoryProductsParser)} ElapseTime: {sw.Elapsed.ToString()}");
        }
    }
}
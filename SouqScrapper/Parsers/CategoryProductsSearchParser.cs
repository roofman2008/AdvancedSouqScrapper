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
using System.Globalization;
using System.Threading;

namespace SouqScrapper.Parsers
{
    public class CategoryProductsSearchParser : IParser
    {
        private Stopwatch sw = new Stopwatch();

        public void Process(Website website)
        {
            //ThreadPool.SetMaxThreads(int.MaxValue, int.MaxValue);
            Console.WriteLine($"{nameof(CategoryProductsSearchParser)} Start");
            sw.Start();
            var categories = website.CategoryGroups.FlattenCategories();

            Parallel.ForEach(categories/*, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }*/, (category) =>
            {
                category.Products = new List<Product>();
                Console.WriteLine(category.Name);
                string categoryId = category.Url.ExtractCategoryId();

                var gridNativeObjs = SouqApi.SearchProducts(categoryId);

                category.HasRedirection = !string.IsNullOrEmpty(gridNativeObjs.First().redirect_url);
                category.Query = !category.HasRedirection ? gridNativeObjs.First().jsonData.meta_data.query : null;
                category.RedirectQuery =
                    category.HasRedirection ? gridNativeObjs.First().jsonData.meta_data.query : null;
                category.RedirectUrl = category.HasRedirection ? gridNativeObjs.First().redirect_url : null;
                category.ExpectedProductCount = gridNativeObjs.Count() * SouqApiConstants.ProductSectionListLimit;

                foreach (var gridObj in gridNativeObjs)
                {
                    var productsNative = gridObj.jsonData.units;

                    foreach (var productNative in productsNative)
                    {
                        var discount = productNative.discount.ToString();
                        var price = productNative.price / 100;
                        var market_price = Math.Abs(productNative.market_price) > 0f
                            ? (double?) (productNative.market_price / 100)
                            : null;

                        Product product = new Product()
                        {
                            Category = category,
                            Id = Guid.NewGuid(),
                            UnitId = productNative.unit_id.ToString(),
                            Title = productNative.title,
                            ProductId = productNative.item_id.ToString(),
                            Url = productNative.primary_link,
                            ImageUrl = productNative.image_url,
                            Manufacturer = productNative.manufacturer.ToTitleCase(),
                            Ean = productNative.ean.FlattenString(),
                            IsFreeShipping = productNative.free_shipping_eligiblity,
                            CurrentPrice = price,
                            //MarketPrice = market_price,
                            //IsRevisioned = productNative.revisioning == "revisioned",
                            //SalesRank = productNative.sales_rank,
                            IsSouqFulfiled = productNative.is_fbs,
                            IsBundled = productNative.bundle_label != "false",
                            //HasDiscountFlag = discount != "False",
                            //DiscountFlag = discount != "False" ? discount : string.Empty,
                            //DiscountFlagPercentage = discount != "False"
                            //    ? int.Parse(discount.ToLower().Replace(" %", ""))
                            //    : 0,
                            Currency = gridObj.jsonData.meta_data.currency,
                            //Page = gridObj.page,
                            //Section = gridObj.section
                        };

                        if (category.Products.All(l => l.Url != product.Url))
                            category.Products.Add(product);
                    }
                }

                category.ActualProductCount = category.Products.Count;
            });

            //categories.First(l=> l.Products != null && l.Products.Count > 0).Products = categories.First(l => l.Products != null && l.Products.Count > 0).Products.Take(50).ToList();

            sw.Stop();
            Console.WriteLine($"{nameof(CategoryProductsSearchParser)} ElapseTime: {sw.Elapsed.ToString()}");
            Console.ReadLine();
        }
    }
}
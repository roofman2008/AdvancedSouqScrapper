using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SouqScrapper.ApiModels;
using SouqScrapper.Core;
using SouqScrapper.Helpers;
using SouqScrapper.Models;

namespace SouqScrapper.Parsers
{
    public class ProductParser : IParser
    {
        private readonly Semaphore _semaphore_SellerQuery = new Semaphore(1, 1);
        private readonly Semaphore _semaphore_AttributeQuery = new Semaphore(1, 1);
        private readonly Semaphore _semaphore_NonIndexedProductsQuery = new Semaphore(1, 1);
        private IEnumerable<Product> products = null;
        private ICollection<Category> categories = null;
        private ICollection<City> cities = null;
        private ConcurrentBag<Seller> sellers = null;
        private ConcurrentBag<ProductAttributeDefinition> attributeDefinitons = null;
        private ConcurrentBag<Product> nonIndexedProducts = null;
        private int index = 0;
        private int finish = 0;
        private DateTime start = DateTime.Now;
        private object lockObj = new object();
        private Website website = null;

        private void Initialize(Website website)
        {
            cities = website.Cities;
            categories = website.Categories;

            products = website.Categories.FlattenProducts()
                //.Where(l => l.Url.Contains(
                //    "https://egypt.souq.com/eg-en/sofa-cover-sleeved-single-2712-brown-38699936/i/"))
                .Shuffle();

            sellers = new ConcurrentBag<Seller>();
            attributeDefinitons = new ConcurrentBag<ProductAttributeDefinition>();
            nonIndexedProducts = new ConcurrentBag<Product>();
            index = 0;
            finish = 0;
            start = DateTime.Now;

            this.website = website;
        }

        public void Process(Website website)
        {
            Console.WriteLine($"{nameof(ProductParser)} Start");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Initialize(website);

            Parallel.ForEach(products, new ParallelOptions(){MaxDegreeOfParallelism = 50}, (product, state) =>
            //foreach (var product in products)
            {
                try
                {
                    ParseLogic(product, null);
                }
                catch (Exception ex)
                {
                    if (!_semaphore_SellerQuery.WaitOne(0))
                        _semaphore_SellerQuery.Release();

                    if (!_semaphore_AttributeQuery.WaitOne(0))
                        _semaphore_AttributeQuery.Release();

                    if (!_semaphore_NonIndexedProductsQuery.WaitOne(0))
                        _semaphore_NonIndexedProductsQuery.Release();

                    //state.Stop();
                    Debugger.Break();
                }
            });

            website.Sellers = sellers.ToList();
            website.AttributeDefinitions = attributeDefinitons.ToList();

            sw.Stop();
            Console.WriteLine($"{nameof(ProductParser)} ElapseTime: {sw.Elapsed.ToString()}");
            //Console.ReadLine();
        }

        private void ParseLogic(Product product, Product mainProduct)
        {
            Stopwatch sw_tmp = new Stopwatch();
            sw_tmp.Start();
            var id = index++;
            Console.WriteLine(id);
            List<string> discoveriedProducts = new List<string>();

            var productBucket = SouqApi.GetProductBucket(product.Url.ExtractProductFullId());

            if (productBucket == null)
                return;

            var productNative = SouqApi.GetProduct(product.Url.ExtractProductFullId());

            if (productNative == null)
                return;

            ParseProductBucket(product, productBucket);

            try
            {
                ParseProductBody(product, productNative.body, discoveriedProducts);

                if (mainProduct != null)
                {
                    discoveriedProducts = discoveriedProducts
                        .Where(l => l != mainProduct.Url)
                        .Where(l => mainProduct.ProductConfigurations.All(c => c.Url != l))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Inconsist Product Body & Bucket")
                {
                    product.IsFaulty = true;
                    //website.Sellers.Remove(product.Seller);
                    return;
                }
            }

            if (productNative.bundles != null)
                ParseProductBundles(product, productNative.bundles);

            var productReviewsNative = SouqApi.GetProductReview(product.ProductId);
            ParseProductReviews(product, productReviewsNative);

            ParseProductDeliveryInfo(product);

            if (mainProduct == null)
                ParseDiscoveredProducts(product, discoveriedProducts);
            else
                ParseDiscoveredProducts(mainProduct, discoveriedProducts);

            sw_tmp.Stop();
            Console.WriteLine($"Task({id}) ElapseTime: {sw_tmp.Elapsed.ToString()}");

            lock (lockObj)
            {
                finish++;
                //var totals = (int)(totalSpan.TotalSeconds / finished.Count);
                //Console.WriteLine($"{nameof(ProductParser)} AvgElapseTime: {new TimeSpan(0,0,totals).ToString()} + Count: {finished.Count}");
                Console.WriteLine(
                    $@"{nameof(ProductParser)} ElapseTime: {DateTime.Now - start} + Count: {finish} +
                                    Task Time: {(DateTime.Now - start).TotalSeconds / finish} [Sec]");
            }
        }

        private void ParseProductBucket(Product product, SouqProductBucket bucket)
        {
            product.ProductId = bucket.Page_Data.product.id_item.ToString();
            product.UnitId = bucket.Page_Data.product.id_unit;
            product.StockCount = bucket.Page_Data.product.quantity;
            product.Title = bucket.Page_Data.product.name;
            product.Ean = bucket.Page_Data.s_ean;
            product.Currency = bucket.Page_Data.product.currencyCode;
            product.Manufacturer = bucket.Page_Data.product.brand;

            #region Category

            if (product.Category == null)
            {
                try
                {
                    product.Category =
                        website.Categories.Single(l => l.Name == bucket.Page_Data.product.category);

                    if (product.Category.Products == null)
                        product.Category.Products = new List<Product>();

                    product.Category.Products.Add(product);
                }
                catch (Exception e)
                {
                    Debugger.Break();
                    throw;
                }
            }

            #endregion

            #region Seller

            var sellerId = bucket.Page_Data.product.id_seller;
            _semaphore_SellerQuery.WaitOne();

            try
            {
                if (sellers.All(l => l.UserCode != sellerId))
                {
                    var sellerObj = new Seller()
                    {
                        Id = Guid.NewGuid(),
                        Name = bucket.Page_Data.product.seller,
                        UserCode = sellerId,
                        Products = new List<Product>()
                    };

                    sellers.Add(sellerObj);
                    sellerObj.Products.Add(product);
                    product.Seller = sellerObj;
                }
                else
                {
                    var sellerObj = sellers.Single(l => l.UserCode == sellerId);
                    sellerObj.Products.Add(product);
                    product.Seller = sellerObj;
                }
            }
            catch (Exception e)
            {
                _semaphore_SellerQuery.Release();
                Debugger.Break();
                throw;
            }

            _semaphore_SellerQuery.Release();

            #endregion
        }

        private void ParseProductBody(Product product, string bodyNative, List<string> discovered)
        {
            var document = new HtmlDocument();
            document.LoadHtml(bodyNative);

            #region Stock
            var outstockNode = document.DocumentNode.Descendants().SingleOrDefaultByNameNClass("h5", "notice");
            var stockWarningNode = document.DocumentNode.Descendants().SingleOrDefaultByNameNClass("div", "unit-labels");

            if (outstockNode != null && outstockNode.InnerText.TrimStart().TrimEnd() == "This item is currently out of stock")
            {
                product.Availability = ProductAvailability.OutStock;
                product.StockCount = null;
            }
            else if (stockWarningNode != null && stockWarningNode.InnerText.TrimStart().TrimEnd() != "")
            {
                var stockCount = int.Parse(stockWarningNode.InnerText.Replace("Only ", "")
                    .Replace(" left in stock!", "").TrimStart().TrimEnd());

                product.Availability = ProductAvailability.NearOutStock;
                product.StockCount = stockCount;
            }
            else
            {
                product.Availability = ProductAvailability.InStock;
            }
            #endregion

            #region Rating
            var ratingNode = document.DocumentNode.Descendants()
                .SingleByNameNContainClass("div", new[] {"product-rating"});

            var averageRatingNode = ratingNode.Descendants().SingleByNameNContainClass("div", new[] {"avg"});
            var stars5RatingNode = ratingNode.Descendants().FindByNameNContainClass("div", new[] {"rate-avg"})
                .SelectMany(l=>l.Descendants())
                .SingleOrDefaultByNameNContainClass("span", new[] {"label", "starRating_5"});
            var stars4RatingNode = ratingNode.Descendants().FindByNameNContainClass("div", new[] { "rate-avg" })
                .SelectMany(l => l.Descendants())
                .SingleOrDefaultByNameNContainClass("span", new[] { "label", "starRating_4" });
            var stars3RatingNode = ratingNode.Descendants().FindByNameNContainClass("div", new[] { "rate-avg" })
                .SelectMany(l => l.Descendants())
                .SingleOrDefaultByNameNContainClass("span", new[] { "label", "starRating_3" });
            var stars2RatingNode = ratingNode.Descendants().FindByNameNContainClass("div", new[] { "rate-avg" })
                .SelectMany(l => l.Descendants())
                .SingleOrDefaultByNameNContainClass("span", new[] { "label", "starRating_2" });
            var stars1RatingNode = ratingNode.Descendants().FindByNameNContainClass("div", new[] { "rate-avg" })
                .SelectMany(l => l.Descendants())
                .SingleOrDefaultByNameNContainClass("span", new[] { "label", "starRating_1" });
            var totalReviewsNode = ratingNode.Descendants().SingleOrDefaultByNameNClass("div", "reviews-total");
            var recommendationNode = ratingNode.Descendants().SingleOrDefaultByNameNClass("span", "messaging");

            var averageRateValue = float.Parse(averageRatingNode.InnerText.TrimStart().TrimEnd());

            if (averageRateValue > 0)
            {
                var stars5RatingValue = int.Parse(stars5RatingNode.InnerText.Replace(",", "").Replace("(", "").Replace(")", ""));
                var stars4RatingValue = int.Parse(stars4RatingNode.InnerText.Replace(",", "").Replace("(", "").Replace(")", ""));
                var stars3RatingValue = int.Parse(stars3RatingNode.InnerText.Replace(",", "").Replace("(", "").Replace(")", ""));
                var stars2RatingValue = int.Parse(stars2RatingNode.InnerText.Replace(",", "").Replace("(", "").Replace(")", ""));
                var stars1RatingValue = int.Parse(stars1RatingNode.InnerText.Replace(",", "").Replace("(", "").Replace(")", ""));
                var totalRatingValue =
                    int.Parse(totalReviewsNode.InnerText.Replace("ratings", "").Replace("rating", "").TrimStart().TrimEnd());
                var recommendationValue = recommendationNode.InnerText.TrimStart().TrimEnd() != ""
                    ? (float?)float.Parse(recommendationNode.InnerText.TrimStart().TrimEnd())
                    : null;

                ProductRate productRate = new ProductRate()
                {
                    AverageRate = averageRateValue,
                    Rate5Count = stars5RatingValue,
                    Rate4Count = stars4RatingValue,
                    Rate3Count = stars3RatingValue,
                    Rate2Count = stars2RatingValue,
                    Rate1Count = stars1RatingValue,
                    TotalRateCount = totalRatingValue,
                    RecommendationPercentage = recommendationValue
                };

                product.HasRating = true;
                product.Rate = productRate;
            }
            #endregion

            #region Price

            if (product.Availability != ProductAvailability.OutStock)
            {
                var priceNode = document.DocumentNode.Descendants()
                    .SingleByNameNContainClass("div", new[] {"price-container"});
                var currentPriceNode = priceNode.Descendants().SingleByNameNContainClass("h3", new[] {"price", "is"});
                var absolutePriceNode = priceNode.Descendants().SingleOrDefaultByNameNClass("span", "was");
                var savePriceNode = priceNode.Descendants().SingleOrDefaultByNameNClass("span", "saved ")?.Descendants()
                    .SingleByNameNClass("span", "noWrap");

                var currentPriceValue = double.Parse(currentPriceNode.InnerText
                    .Replace("&nbsp;", "")
                    .ToUpper().Replace("EGP", "")
                    .TrimStart().TrimEnd());

                product.CurrentPrice = currentPriceValue;

                if (absolutePriceNode != null && savePriceNode != null)
                {
                    var absolutePriceValue =
                        double.Parse(absolutePriceNode.InnerText
                            .Replace("&nbsp;", "")
                            .ToUpper().Replace("EGP", "").TrimStart().TrimEnd());
                    var savingPriceValue =
                        double.Parse(savePriceNode.InnerText
                            .Replace("&nbsp;", "")
                            .ToUpper().Replace("EGP", "").TrimStart().TrimEnd());

                    product.HasDiscount = true;
                    product.AbsolutePrice = absolutePriceValue;
                    product.DiscountSaving = savingPriceValue;
                    product.DiscountPercentage =
                        (int?) Math.Ceiling((1.0d - (product.CurrentPrice / product.AbsolutePrice.Value)) * 100);
                }
                else
                {
                    product.HasDiscount = false;
                    product.AbsolutePrice = null;
                    product.DiscountSaving = null;
                    product.DiscountPercentage = null;
                }
            }

            #endregion

            #region Condition

            if (product.Availability != ProductAvailability.OutStock)
            {
                var conditionNode = document.DocumentNode.Descendants().SingleByNameNClass("dd", "unit-condition");
                product.Condition = conditionNode.InnerText.TrimStart().TrimEnd();
            }

            #endregion

            #region Seller

            if (product.Availability != ProductAvailability.OutStock)
            {
                var sellerNode = document.DocumentNode.Descendants().SingleByNameNClass("span", "unit-seller-link")
                    .Descendants().SingleByName("a");
                var sellerNotesNode = document.DocumentNode.Descendants().SingleOrDefaultByNameNClass("span", "seller-notes");
                var sellerName = sellerNode.InnerText.TrimStart().TrimEnd();
                var sellerPortfoliUrl = sellerNode.Attributes["href"].Value;
                var sellerId = sellerPortfoliUrl.ExtractSellerId();

                Console.WriteLine(sellerId);

                _semaphore_SellerQuery.WaitOne();

                try
                {
                    if (sellers.All(l => l.Name != sellerName))
                    {
                        //Debugger.Break();
                        throw new Exception("Inconsist Product Body & Bucket");
                    }
                    else
                    {
                        var sellerObj = sellers.Single(l => l.Name == sellerName);
                        sellerObj.Url = sellerPortfoliUrl;
                        sellerObj.UserId = sellerId;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message != "Inconsist Product Body & Bucket")
                        Debugger.Break();

                    _semaphore_SellerQuery.Release();

                    throw;
                }

                _semaphore_SellerQuery.Release();

                product.SellerNote = sellerNotesNode?.InnerText.TrimStart().TrimEnd();
            }

            #endregion

            #region Connections

            var connectionsNode = document.DocumentNode.Descendants().SingleOrDefaultByNameNClass("div", "product-connections");

            if (connectionsNode != null)
            {
                var connectionsDictionaryNodes = connectionsNode.Descendants()
                    .FindByNameNClass("div", "connection-stand")
                    .ToDictionary(l => l.PreviousSibling.PreviousSibling, l => l);

                foreach (var connectionNode in connectionsDictionaryNodes)
                {
                    var connectionsValuesNode =
                        connectionNode.Value.Descendants().FindByNameNClass("a", "value size-value");
                    var connectionTitle = connectionNode.Key.InnerText.TrimStart().TrimEnd();
                    var connectionsValue = connectionsValuesNode.Select(l => new
                    {
                        Url = l.Attributes["data-url"].Value,
                        IsEnabled = bool.Parse(l.Attributes["data-enabled"].Value),
                        Value = l.InnerText.TrimStart().TrimEnd()
                    }).ToList();

                    var discoveredProducts = connectionsValue.Where(l => !l.IsEnabled).Select(l => l.Url);

                    foreach (var discoveredProduct in discoveredProducts)
                        if (discovered.All(l => l != discoveredProduct))
                            discovered.Add(discoveredProduct);

                    _semaphore_AttributeQuery.WaitOne();

                    ProductAttributeDefinition definition =
                        attributeDefinitons.SingleOrDefault(l => l.Name == connectionTitle);

                    _semaphore_AttributeQuery.Release();

                    if (definition == null)
                    {
                        definition = new ProductAttributeDefinition()
                        {
                            Id = Guid.NewGuid(),
                            Name = connectionTitle,
                            Attributes = new List<ProductAttribute>()
                        };
                        attributeDefinitons.Add(definition);
                    }

                    definition.Values = definition.Values ?? new List<ProductAttributeValue>();
                    var activeConnectionValue = connectionsValue.Single(l => l.IsEnabled);
                    var attributeValue = definition.Values.SingleOrDefault(l => l.Value == activeConnectionValue.Value);

                    if (attributeValue == null)
                    {
                        attributeValue = new ProductAttributeValue()
                        {
                            Id = Guid.NewGuid(),
                            ProductAttributeDefinition = definition,
                            Value = activeConnectionValue.Value,
                            Type = CheckType(activeConnectionValue.Value),
                            Attribute = new List<ProductAttribute>()
                        };

                        definition.Values.Add(attributeValue);
                    }

                    var attributeInstance = new ProductAttribute()
                    {
                        Definition = definition,
                        Value = attributeValue,
                        IsConfiguration = true,
                    };

                    if (product.Attributes == null)
                        product.Attributes = new List<ProductAttribute>();

                    product.Attributes.Add(attributeInstance);
                    attributeValue.Attribute.Add(attributeInstance);
                    definition.Attributes.Add(attributeInstance);
                }
            }

            #endregion

            #region Attributes

            var specsContainerNode = document.DocumentNode.Descendants().SingleOrDefaultByNameNAttribute("div", "id", "specs-full") ??
                                     document.DocumentNode.Descendants().SingleByNameNAttribute("div", "id", "specs-short");

            var specsNode = specsContainerNode.ChildNodes.SingleByNameNClass("dl", "stats");
            var specsDictionaryNodes =
                specsNode.ChildNodes.FindByName("dd").ToDictionary(l => l.PreviousSibling, l => l);

            foreach (var specNode in specsDictionaryNodes)
            {
                var title = specNode.Key.InnerText.TrimStart().TrimEnd();
                var valueText = specNode.Value.InnerText.TrimStart().TrimEnd().Replace("&nbsp;Read more", "");
                var valueClass = specNode.Value.ChildNodes.SingleOrDefaultByExcludeClass(new[]{ "expand", "readmore", "collapsed" })?.Attributes["class"]?.Value ?? null;

                if (valueClass != null)
                    switch (valueClass)
                    {
                        case "fi-x":
                            valueText = "False";
                            break;
                        case "fi-check":
                            valueText = "True";
                            break;
                        default:
                            //Debugger.Break();
                            break;
                    }

                _semaphore_AttributeQuery.WaitOne();

                ProductAttributeDefinition definition =
                    attributeDefinitons.SingleOrDefault(l => l.Name == title);

                _semaphore_AttributeQuery.Release();

                if (definition == null)
                {
                    definition = new ProductAttributeDefinition()
                    {
                        Id = Guid.NewGuid(),
                        Name = title,
                        Attributes = new List<ProductAttribute>()
                    };
                    attributeDefinitons.Add(definition);
                }

                definition.Values = definition.Values ?? new List<ProductAttributeValue>();
                var attributeValue = definition.Values.SingleOrDefault(l => l.Value == valueText);

                if (attributeValue == null)
                {
                    attributeValue = new ProductAttributeValue()
                    {
                        Id = Guid.NewGuid(),
                        ProductAttributeDefinition = definition,
                        Value = valueText,
                        Type = CheckType(valueText),
                        Attribute = new List<ProductAttribute>()
                    };

                    definition.Values.Add(attributeValue);
                }

                if (product.Attributes == null)
                    product.Attributes = new List<ProductAttribute>();

                if (product.Attributes.All(l => l.Definition.Name != definition.Name))
                {
                    var attributeInstance = new ProductAttribute()
                    {
                        Definition = definition,
                        Value = attributeValue,
                        IsConfiguration = false,
                    };

                    product.Attributes.Add(attributeInstance);
                    attributeValue.Attribute.Add(attributeInstance);
                    definition.Attributes.Add(attributeInstance);
                }
            }

            #endregion

            #region ProductImages

            var imagesContainerNode = document.DocumentNode.Descendants().SingleOrDefaultByNameNClass("div", "vip-item-img-container");

            if (imagesContainerNode != null)
            {
                var imagesNodes = imagesContainerNode.Descendants().FindByNameNAttribute("img", "data-url");
                product.Hyperlinks = new List<Hyperlink>();

                foreach (var imageNode in imagesNodes)
                {
                    var imageUrl = imageNode.Attributes["data-url"].Value;
                    var hyperlink = new Hyperlink()
                    {
                        Id = Guid.NewGuid(),
                        Url = imageUrl
                    };
                    product.Hyperlinks.Add(hyperlink);
                }

                product.ImageUrl = product.Hyperlinks.FirstOrDefault()?.Url;
            }
            else
            {
                var imageContainerNode = document.DocumentNode.Descendants()
                    .SingleByNameNContainClass("div", new[] {"vip-outofstock-item-img-container"});
                var imageNode = imageContainerNode.ChildNodes.SingleByName("img");
                product.ImageUrl = imageNode.Attributes["src"].Value;
            }

            #endregion

            #region Fulfiled

            var isFulfiledNode = document.DocumentNode.Descendants()
                .SingleOrDefaultByNameNContainAttribute("span", "data-toggle", "Fullfilled-tooltip");

            if (isFulfiledNode != null)
                product.IsSouqFulfiled = true;

            #endregion

            #region Shipping

            var isShippingFree = document.DocumentNode.Descendants()
                .SingleOrDefaultByNameNContainAttribute("a", "data-open", "freeShipping");

            if (isShippingFree != null)
                product.IsFreeShipping = true;

            #endregion
        }

        private void ParseProductBundles(Product product, IEnumerable<SouqBundle> bundlesNative)
        {
            product.IsBundled = true;

            List<ProductBundle> productBundles = new List<ProductBundle>();
            
            foreach (var bundleNative in bundlesNative)
            {
                double bundle_discount = double.Parse(bundleNative.discount) / 100;
                double bundle_currentPrice = bundleNative.price / 100;
                double bundle_absolutePrice = bundle_currentPrice + bundle_discount;

                var bundle = new ProductBundle()
                {
                    Title = bundleNative.title,
                    Product = product,
                    AbsolutePrice = bundle_absolutePrice,
                    CurrentPrice = bundle_currentPrice,
                    Discount = bundle_discount,
                    Currency = bundleNative.country_currency,
                    BundleId = bundleNative.id_bundle.ToString(),
                    Quantitiy = bundleNative.qty,
                    Id = Guid.NewGuid()
                };

                List<ProductBundleUnit> bundleUnits = new List<ProductBundleUnit>();

                if (bundleNative.units != null)
                {
                    foreach (var bundleUnitNative in bundleNative.units)
                    {
                        var unit_discount = double.Parse(bundleUnitNative.discount) / 100;
                        var unit_absolutePrice = double.Parse(bundleUnitNative.itemOriginalPrice) / 100;
                        var unit_currentPrice = unit_absolutePrice - unit_discount;

                        _semaphore_NonIndexedProductsQuery.WaitOne();

                        var unit = new ProductBundleUnit()
                        {
                            Id = Guid.NewGuid(),
                            Product = products.Concat(nonIndexedProducts)
                                .SingleOrDefault(l => l.Url == bundleUnitNative.unitUrl.ToString()),
                            Bundle = bundle,
                            CurrentPrice = unit_currentPrice,
                            Discount = unit_discount,
                            Quantity = bundleUnitNative.qty,
                            AbsolutePrice = unit_absolutePrice
                        };

                        _semaphore_NonIndexedProductsQuery.Release();

                        if (unit.Product == null)
                        {
                            //Debugger.Break();

                            unit.Product = new Product()
                            {
                                Id = Guid.NewGuid(),
                                Url = bundleUnitNative.unitUrl
                            };

                            nonIndexedProducts.Add(unit.Product);

                            ParseLogic(unit.Product, null);
                        }

                        bundleUnits.Add(unit);
                    }

                    bundle.BundleUnits = bundleUnits.ToList();
                }

                productBundles.Add(bundle);
            }

            product.Bundles = productBundles.ToList();
        }

        private void ParseProductReviews(Product product, IEnumerable<SouqProductReview> reviewsNative)
        {
            if (product.Reviews == null)
                product.Reviews = new List<ProductReview>();

            foreach (var reviewNative in reviewsNative)
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(reviewNative.body);

                var reviewsNodes = document.DocumentNode.Descendants().FindByNameNAttribute("li", "data-review");

                foreach (var reviewNode in reviewsNodes)
                {
                    var reviewId = reviewNode.Attributes["data-review"].Value;
                    var headerNode = reviewNode.ChildNodes.SingleByName("header");
                    var articleNode = reviewNode.ChildNodes.SingleByName("article");
                    var purchasedNode = reviewNode.Descendants()
                        .SingleOrDefaultByNameNContainClass("div", new[] {"purchasedBadge"});
                    var headerInfoNode = headerNode.Descendants()
                        .SingleByNameNContainClass("div", new[] {"clearfix", "space", "by-date" });
                    var reviewerNameNode = headerInfoNode.Descendants().SingleByName("strong");
                    var reviewerDateNode = headerInfoNode.ChildNodes.SingleByNameNClass("span", "date");

                    var review = new ProductReview()
                    {
                        Id = Guid.NewGuid(),
                        Product = product,
                        IsPurchased = purchasedNode != null,
                        Username = reviewerNameNode.InnerText.TrimStart().TrimEnd(),
                        Comment = articleNode.InnerText.TrimStart().TrimEnd(),
                        Timestamp = DateTime.Parse(reviewerDateNode.InnerText.TrimStart().TrimEnd())
                    };

                    product.Reviews.Add(review);
                }
            }

            product.ReviewsCount = product.Reviews.Count;
        }

        private void ParseProductDeliveryInfo(Product product)
        {
            object lockObject = new object();

            if (product.Availability == ProductAvailability.OutStock)
                return;

            ConcurrentBag<ProductDelivery> productDeliveries = new ConcurrentBag<ProductDelivery>();

            var token = SouqApi.GetProductAccessTokens(product.Url.ExtractProductFullId())["searchForm"];

            int errorCount = 0;
            int tryCount = cities.Count;
            ManualResetEvent manualResetEvent = new ManualResetEvent(false);
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            bool validToken = true;

            //foreach(var city in cities)
            Parallel.ForEach(cities, (city) =>
            {
                retoken:
                SouqDeliveryInfo deliveryInfo;
                try
                {
                    deliveryInfo = SouqApi.GetDeliveryInfo(city.Code, city.Name, product.UnitId, token.hitsCfs,
                        token.hitsCfsMeta);
                    --tryCount;
                }
                catch (Exception ex)
                {
                    ++errorCount;
                    validToken = false;

                    if (errorCount > 0 && tryCount != errorCount)
                    {
                        manualResetEvent.WaitOne();
                    }
                    else if (errorCount > 0 && tryCount == errorCount)
                    {
                        autoResetEvent.Set();
                        manualResetEvent.Set();
                    }

                    autoResetEvent.WaitOne();

                    if (!validToken)
                    {
                        errorCount = 0;
                        manualResetEvent.Reset();
                        token = SouqApi.GetProductAccessTokens(product.Url.ExtractProductFullId())["searchForm"];
                        validToken = true;
                    }

                    autoResetEvent.Set();
                    goto retoken;
                }

                var canDeliver = deliveryInfo.estimate_by_days != null;
                var daysNo = deliveryInfo.estimate_by_days;

                ProductDelivery delivery = new ProductDelivery()
                {
                    City = city,
                    Id = Guid.NewGuid(),
                    EstimatedDays = daysNo,
                    CanDeliver = canDeliver
                };

                productDeliveries.Add(delivery);
            });

            product.Deliveries = productDeliveries.ToList();
        }

        private void ParseDiscoveredProducts(Product mainProduct, List<string> discovered)
        {
            if (discovered.Count == 0)
                return;

            int tmp_Index = 0;
            do
            {
                var productUrl = discovered[tmp_Index++];

                if (mainProduct.ProductConfigurations == null || mainProduct.ProductConfigurations.All(l => l.Url != productUrl))
                {
                    var product = new Product()
                    {
                        Id = Guid.NewGuid(),
                        Url = productUrl,
                        Category = mainProduct.Category,
                        ProductParentConfiguration = mainProduct,
                    };

                    mainProduct.Category.Products.Add(product);

                    if (mainProduct.ProductConfigurations == null)
                        mainProduct.ProductConfigurations = new List<Product>();

                    mainProduct.ProductConfigurations.Add(product);

                    ParseLogic(product, mainProduct);
                }
                else
                {
                    tmp_Index++;
                }
            } while (tmp_Index < discovered.Count);
        }

        private ProductAttrbuteValueType CheckType(string value)
        {
            var isNumber = float.TryParse(value, out float number);

            if (isNumber)
            {
                var flooredValue = Math.Floor(number);
                return Math.Abs(flooredValue - number) < 0.0001 ? ProductAttrbuteValueType.Integer : ProductAttrbuteValueType.Float;
            }

            return ProductAttrbuteValueType.String;
        }
    }
}
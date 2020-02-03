using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SouqScrapper.ApiModels;
using SouqScrapper.Helpers;
using SouqScrapper.LinkGenerators;
using SouqScrapper.Models;
using SouqScrapper.Parsers;
using SouqScrapper.Stores;

namespace SouqScrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            //ProxyHelper.VerifyProxies();
            var website = new Website();

            //var bucket = SouqApi.GetProductBucket("xiaomi-redmi-6-dual-sim-32-gb-3-gb-ram-4g-lte-gold-international-version-36234912");
            //var productReviews = SouqApi.GetProductReview("36234912");
            //var sellerReviews = SouqApi.GetSellerReview("alafreetshop");
            //var cookies = SouqApi.GetProductAccessTokens("xd-design-bobby-anti-theft-backpack-black-24168110");
            CityParser cityParser = new CityParser();
            CategoryParser categoryParser = new CategoryParser();
            CategoryProductsParser categoryProductsParser = new CategoryProductsParser();
            ProductParser productParser = new ProductParser();
            SellerParser sellerParser = new SellerParser();

            cityParser.Process(website);
            categoryParser.Process(website);
            categoryProductsParser.Process(website);
            productParser.Process(website);
            sellerParser.Process(website);

            var x = JsonConvert.SerializeObject(website);
            File.WriteAllText("dump.json", x);
            Console.WriteLine("Dump Done");

            Console.ReadLine();



            //ExcelStore store = new ExcelStore();
            //store.Store(website);

            //var product = website.CategoryGroups.FlattenCategories().FlattenProducts().First();

            //var token = SouqApi.GetProductAccessTokens(product.Url.ExtractProductFullId()).First().Value;
            //var countryObj = website.Cities.First();
            //SouqApi.GetDeliveryInfo(countryObj.Code, countryObj.Name, product.UnitId, token.hitsCfs, token.hitsCfsMeta);


        }
    }
}

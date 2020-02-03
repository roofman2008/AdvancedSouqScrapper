using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using SouqScrapper.ApiModels;
using SouqScrapper.Core;
using SouqScrapper.Helpers;
using SouqScrapper.Models;

namespace SouqScrapper.Parsers
{
    public class CityParser : IParser
    {
        public void Process(Website website)
        {
            Console.WriteLine($"{nameof(CityParser)} Start");

            var nativeObj = SouqApi.GetCities();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(nativeObj.sOptions);

            var options = document.DocumentNode.Descendants().FindByName("option").Skip(1);

            website.Cities = new List<City>();

            foreach (var option in options)
            {
                var country = new City()
                {
                    Id = Guid.NewGuid(),
                    Name = option.InnerText,
                    Code = option.Attributes["value"].Value
                };

                website.Cities.Add(country);   
            }
        }
    }
}
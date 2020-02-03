using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SouqScrapper.ApiModels;
using SouqScrapper.Core;
using SouqScrapper.Helpers;
using SouqScrapper.Models;

namespace SouqScrapper.Parsers
{
    public class SellerParser : IParser
    {
        public void Process(Website website)
        {
            Console.WriteLine($"{nameof(SellerParser)} Start");

            Parallel.ForEach(website.Sellers/*, new ParallelOptions() { MaxDegreeOfParallelism = 1 }*/, (seller) =>
            {
                Console.WriteLine(seller.Url);
                try
                {
                    ProcessSeller(seller);
                }
                catch (Exception e)
                {
                    Debugger.Break();
                }
            });
        }

        private void ProcessSeller(Seller seller)
        {
            var sellerNative = SouqApi.GetSellerProfile(seller.UserId);
            var sellerInfoAvailable = !sellerNative.DocumentNode.Descendants().Any(l => l.Name == "div" && l.InnerText.Contains("This seller has no ratings yet"));
            var sellerInfoError = !sellerNative.DocumentNode.Descendants()
                .AnyByNameNContainClass("div", new[] { "grouped-list", "rating-tabs" });


            if (sellerInfoError)
            {
                seller.IsFaulty = true;
                seller.HasNoData = true;
                return;
            }

            if (sellerInfoAvailable)
            {
                var sellerDetailsNode = sellerNative.DocumentNode.Descendants()
                    .SingleByNameNContainClass("div", new[] { "grouped-list", "rating-tabs" });
                var sellerInfoNode = sellerDetailsNode.ChildNodes.FindByName("div").First();
                var sellerStatsNode = sellerDetailsNode.ChildNodes.FindByName("div").Last();
                var sellerNameUrlNode =
                    sellerInfoNode.Descendants().SingleByName("h6").ChildNodes.SingleByName("a");
                var sellerRateNode = sellerInfoNode.Descendants().SingleByNameNClass("i", "star-rating-svg")
                    .ChildNodes.SingleByName("i");
                var sellerDateNode = sellerNameUrlNode.ParentNode.ParentNode.ParentNode.ChildNodes.FindByName("div")
                    .Last();
                var sellerRatingTabs = sellerDetailsNode.Descendants().SingleByNameNClass("div", "tabs-content")
                    .ChildNodes.FindByName("section");

                var sellerRate =
                    float.Parse(sellerRateNode.Attributes["style"].Value.Replace("width:", "").Replace("%", "")) /
                    100f * 5f;
                var sellerDate =
                    DateTime.Parse(sellerDateNode.InnerText.TrimStart().TrimEnd().Replace("Member since: ", ""));

                SellerRate[] rates = new SellerRate[3];
                int index = 0;

                foreach (var sellerRatingTab in sellerRatingTabs)
                {
                    var positiveNode = sellerRatingTab.Descendants().FindByName("ul").FirstOrDefault(l => l.InnerText.Contains("Positive"));
                    var negativeNode = sellerRatingTab.Descendants().FindByName("ul").FirstOrDefault(l => l.InnerText.Contains("Negative"));
                    var totalRatingNodeNode = sellerRatingTab.ChildNodes.SingleOrDefaultByName("div");

                    SellerRate rate = null;

                    if (totalRatingNodeNode != null)
                    {
                        rate = new SellerRate();
                        rate.TotalRating = int.Parse(totalRatingNodeNode.InnerText.TrimStart().TrimEnd()
                            .Replace("Total Ratings: ", ""));
                    }

                    if (positiveNode != null)
                    {
                        var postivePercentageNode = positiveNode.ChildNodes.FindByName("li").Last();
                        rate.PositiveRatePercentage =
                            int.Parse(postivePercentageNode.InnerText.TrimStart().TrimEnd().Replace("%", ""));
                    }

                    if (negativeNode != null)
                    {
                        var negativePercentageNode = negativeNode.ChildNodes.FindByName("li").Last();
                        rate.NegativeRatePercentage =
                            int.Parse(negativePercentageNode.InnerText.TrimStart().TrimEnd().Replace("%", ""));
                    }

                    rates[index++] = rate;
                }

                seller.AverageRate = sellerRate;
                seller.JoinDate = sellerDate;
                seller.LastYearRate = rates[0];
                seller.LastQuarterRate = rates[1];
                seller.LastMonthRate = rates[2];
            }
            else
            {
                seller.HasNoData = true;
            }
        }
    }
}
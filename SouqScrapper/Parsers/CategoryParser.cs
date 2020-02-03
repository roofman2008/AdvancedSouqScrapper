using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HtmlAgilityPack;
using SouqScrapper.ApiModels;
using SouqScrapper.Core;
using SouqScrapper.Helpers;
using SouqScrapper.Models;

namespace SouqScrapper.Parsers
{
    public class CategoryParser : IParser
    {
        private Stopwatch sw = new Stopwatch();

        public void Process(Website website)
        {
            Console.WriteLine($"{nameof(CategoryParser)} Start");
            sw.Start();
            var categoryGroups = new List<CategoryGroup>();
            var categories = new List<Category>();

            var document = SouqApi.GetCategories();
            var divNodes = document.DocumentNode.Descendants().FindByNameNClass("div", "grouped-list");

            foreach (var divNode in divNodes)
            {
                var categoryGroup = new CategoryGroup
                {
                    Id = Guid.NewGuid(),
                    Name = divNode.PreviousSibling.InnerText.Cleanify(),
                    Parent = null,
                    Categories = new List<Category>(),
                    CategoryGroups = new List<CategoryGroup>()
                };

                ParseRecursively(categories, categoryGroup, divNode);

                categoryGroups.Add(categoryGroup);
            }

            website.CategoryGroups = categoryGroups;
            website.Categories = categories;
            sw.Stop();
            Console.WriteLine($"{nameof(CategoryParser)} ElapseTime: {sw.Elapsed.ToString()}");
        }

        private void ParseRecursively(List<Category> categories, CategoryGroup categoryGroup, HtmlNode container)
        {
            var ulNode = container.ChildNodes.SingleByName("ul");
            var liNodes = ulNode.ChildNodes.FindByName("li");

            foreach (var liNode in liNodes)
            {
                if (liNode.Attributes.Contains("class") &&
                    liNode.Attributes["class"].Value == "parent")
                {
                    var aNode = liNode.ChildNodes.SingleByName("a");

                    var childCategoryGroup = new CategoryGroup()
                    {
                        Id = Guid.NewGuid(),
                        Name = aNode.InnerText.Cleanify(),
                        Parent = categoryGroup,
                        Categories = new List<Category>(),
                        CategoryGroups = new List<CategoryGroup>()
                    };

                    ParseRecursively(categories, childCategoryGroup, liNode);

                    categoryGroup.CategoryGroups.Add(childCategoryGroup);
                }
                else
                {
                    var aNode = liNode.ChildNodes.SingleByName("a");
                    var url = aNode.Attributes["href"].Value;

                    if (url.ExtractOperation() == "list" && !url.HasFilterTags())
                    {
                        var category = categories.SingleOrDefault(l => l.Url.ToLower() == aNode.Attributes["href"].Value.ToLower());

                        if (category != null)
                        {
                            category.CategoryGroups.Add(categoryGroup);
                        }
                        else
                        {
                            category = new Category()
                            {
                                Id = Guid.NewGuid(),
                                Name = aNode.InnerText.Cleanify(),
                                Url = aNode.Attributes["href"].Value,
                                CategoryGroups = new List<CategoryGroup>()
                                    {categoryGroup},
                                IsFaulty = false
                            };

                            categories.Add(category);
                        }

                        categoryGroup.Categories.Add(category);
                    }
                }
            }
        }
    }
}
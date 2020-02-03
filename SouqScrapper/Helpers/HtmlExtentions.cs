using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace SouqScrapper.Helpers
{
    public static class HtmlExtentions
    {
        public static int CountByNameNClass<TSource>(this IEnumerable<TSource> source, string name, string @class) where TSource : HtmlNode
        {
            return source.Count(l =>
                l.Name == name && l.Attributes.Contains("class") && l.Attributes["class"].Value == @class);
        }

        public static TSource SingleByNameNClass<TSource>(this IEnumerable<TSource> source, string name, string @class) where TSource : HtmlNode
        {
            return source.Single(l =>
                l.Name == name && l.Attributes.Contains("class") && l.Attributes["class"].Value == @class);
        }

        public static TSource SingleByExcludeClass<TSource>(this IEnumerable<TSource> source, string @class) where TSource : HtmlNode
        {
            return source.Single(l => l.Attributes["class"].Value != @class);
        }

        public static TSource SingleOrDefaultByNameNClass<TSource>(this IEnumerable<TSource> source, string name, string @class) where TSource : HtmlNode
        {
            return source.SingleOrDefault(l =>
                l.Name == name && l.Attributes.Contains("class") && l.Attributes["class"].Value == @class);
        }

        public static IEnumerable<TSource> FindByNameNClass<TSource>(this IEnumerable<TSource> source, string name, string @class) where TSource : HtmlNode
        {
            return source.Where(l =>
                l.Name == name && l.Attributes.Contains("class") && l.Attributes["class"].Value == @class);
        }

        public static IEnumerable<TSource> FindByName<TSource>(this IEnumerable<TSource> source, string name) where TSource : HtmlNode
        {
            return source.Where(l => l.Name == name);
        }

        public static TSource SingleByName<TSource>(this IEnumerable<TSource> source, string name) where TSource : HtmlNode
        {
            return source.Single(l => l.Name == name);
        }

        public static TSource SingleOrDefaultByName<TSource>(this IEnumerable<TSource> source, string name) where TSource : HtmlNode
        {
            return source.SingleOrDefault(l => l.Name == name);
        }

        public static IEnumerable<TSource> FindByNameNAttribute<TSource>(this IEnumerable<TSource> source, string name, string attribute) where TSource : HtmlNode
        {
            return source.Where(l => l.Name == name && l.Attributes.Contains(attribute));
        }

        public static TSource SingleByNameNAttribute<TSource>(this IEnumerable<TSource> source, string name, string attribute, string attributeValue) where TSource : HtmlNode
        {
            return source.Single(l => l.Name == name && l.Attributes.Contains(attribute) && l.Attributes[attribute].Value == attributeValue);
        }

        public static TSource SingleOrDefaultByNameNAttribute<TSource>(this IEnumerable<TSource> source, string name, string attribute, string attributeValue) where TSource : HtmlNode
        {
            return source.SingleOrDefault(l => l.Name == name && l.Attributes.Contains(attribute) && l.Attributes[attribute].Value == attributeValue);
        }

        public static bool AnyByNameNAttribute<TSource>(this IEnumerable<TSource> source, string name, string attribute, string attributeValue) where TSource : HtmlNode
        {
            return source.Any(l => l.Name == name && l.Attributes.Contains(attribute) && l.Attributes[attribute].Value == attributeValue);
        }

        public static TSource SingleByNameNContainAttribute<TSource>(this IEnumerable<TSource> source, string name, string attribute, string attributeValue) where TSource : HtmlNode
        {
            return source.Single(l => l.Name == name && l.Attributes.Contains(attribute) && l.Attributes[attribute].Value.Contains(attributeValue));
        }

        public static TSource SingleOrDefaultByNameNContainAttribute<TSource>(this IEnumerable<TSource> source, string name, string attribute, string attributeValue) where TSource : HtmlNode
        {
            return source.SingleOrDefault(l => l.Name == name && l.Attributes.Contains(attribute) && l.Attributes[attribute].Value.Contains(attributeValue));
        }

        public static TSource SingleByNameNContainClass<TSource>(this IEnumerable<TSource> source, string name, string[] classes) where TSource : HtmlNode
        {
            return source.Single(l =>
                l.Name == name && l.Attributes.Contains("class") &&
                classes.All(c=>l.Attributes["class"].Value.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries).Contains(c)));
        }

        public static bool AnyByNameNContainClass<TSource>(this IEnumerable<TSource> source, string name, string[] classes) where TSource : HtmlNode
        {
            return source.Any(l =>
                l.Name == name && l.Attributes.Contains("class") &&
                classes.All(c => l.Attributes["class"].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(c)));
        }

        public static TSource SingleByExcludeClass<TSource>(this IEnumerable<TSource> source, string[] classes) where TSource : HtmlNode
        {
            return source.Single(l => l.Attributes.Contains("class") &&
                classes.All(c => !l.Attributes["class"].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(c)));
        }

        public static TSource SingleOrDefaultByExcludeClass<TSource>(this IEnumerable<TSource> source, string[] classes) where TSource : HtmlNode
        {
            return source.SingleOrDefault(l => l.Attributes.Contains("class") &&
                                      classes.All(c => !l.Attributes["class"].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(c)));
        }

        public static TSource SingleOrDefaultByNameNContainClass<TSource>(this IEnumerable<TSource> source, string name, string[] classes) where TSource : HtmlNode
        {
            return source.SingleOrDefault(l =>
                l.Name == name && l.Attributes.Contains("class") &&
                classes.All(c => l.Attributes["class"].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Contains(c)));
        }

        public static IEnumerable<TSource> FindByNameNContainClass<TSource>(this IEnumerable<TSource> source, string name, string[] classes) where TSource : HtmlNode
        {
            return source.Where(l => l.Name == name && l.Attributes.Contains("class") &&
                                     classes.All(c =>
                                         l.Attributes["class"].Value.Split(new char[] {' '},
                                             StringSplitOptions.RemoveEmptyEntries).Contains(c)));
        }
    }
}
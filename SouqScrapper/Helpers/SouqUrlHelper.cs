using System;
using System.Diagnostics;
using System.Linq;

namespace SouqScrapper.Helpers
{
    public static class SouqUrlHelper
    {
        public static string ExtractProductFullId(this string url)
        {
            //https://egypt.souq.com/eg-en/xiaomi-redmi-6-dual-sim-32-gb-3-gb-ram-4g-lte-gold-international-version-36234912/i/

            return url
                .Replace("https://egypt.souq.com/eg-en/", "")
                .Replace("/i/", "");
        }

        public static string ExtractProductId(this string url)
        {
            var fullId = ExtractProductFullId(url);
            var fullIdTokens = fullId.Split(new char[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
            return fullIdTokens.Last();
        }

        public static string NormalizeSellerName(this string sellername)
        {
            return sellername.ToLower();
        }

        public static string ExtractSellerId(this string url)
        {
            return url
                .Replace("https://egypt.souq.com/eg-en/", "")
                .Replace("/p/", "")
                .Replace("profile.html", "");
        }

        public static string ExtractCategoryId(this string url)
        {
            return url
                .Replace("/eg-en//", "/eg-en/")
                .Replace("https://egypt.souq.com/eg-en/", "")
                .Replace("/l/?ref=nav", "")
                .Replace("/l/", "")
                .Replace("/s/", "")
                .Replace(" ", "%20");
        }

        public static string ExtractOperation(this string url)
        {
            var token = url
                .Replace("/eg-en//", "/eg-en/")
                .Replace("https://egypt.souq.com/eg-en/", "")
                .Replace("?ref=nav", "")
                .Replace(" ", "-");

            token = token.Substring(token.LastIndexOf("/", StringComparison.Ordinal) - 2, 3);

            switch (token)
            {
                case "/s/":
                    return "search";
                case "/l/":
                    return "list";
                default:
                    Debugger.Break();
                    return "unknown";
            }
        }

        public static bool HasFilterTags(this string url)
        {
            var token = url
                .Replace("/eg-en//", "/eg-en/")
                .Replace("https://egypt.souq.com/eg-en/", "")
                .Replace("?ref=nav", "")
                .Replace(" ", "-")
                .Replace("/l/?ref=nav", "")
                .Replace("/l/", "")
                .Replace("/s/", "")
                .Replace(" ", "%20");

            return token.Count(l => l == '/') > 0;
        }
    }
}
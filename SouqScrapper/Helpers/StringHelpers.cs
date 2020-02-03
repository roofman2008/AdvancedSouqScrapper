using System.Globalization;

namespace SouqScrapper.Helpers
{
    public static class StringHelpers
    {
        public static string ToTitleCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(str);
        }

        public static string Cleanify(this string str)
        {
            return str
                .Replace("&nbsp; ", "")
                .Replace("&amp; ", "& ")
                .TrimStart()
                .TrimEnd();
        }
    }
}
namespace SouqScrapper.Helpers
{
    public static class UriHelper
    {
        public static string ToPercentageEncoding(this string input, int iterations)
        {
            string tmpInput = input;

            for (int i = 0; i < iterations; i++)
                tmpInput = System.Uri.EscapeDataString(tmpInput);

            return tmpInput;
        }

        public static string ToPercentageDecoding(this string input, int iterations)
        {
            string tmpInput = input;

            for (int i = 0; i < iterations; i++)
                tmpInput = System.Uri.UnescapeDataString(tmpInput);

            return tmpInput;
        }

        public static string UrlClearify(this string input)
        {
            return input
                .Replace(" ", "%20");
        }
    }
}
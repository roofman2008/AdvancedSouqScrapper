namespace SouqScrapper.Models
{
    public class ProductRate
    {
        public float AverageRate { get; set; }
        public int TotalRateCount { get; set; }
        public int Rate5Count { get; set; }
        public int Rate4Count { get; set; }
        public int Rate3Count { get; set; }
        public int Rate2Count { get; set; }
        public int Rate1Count { get; set; }
        public float? RecommendationPercentage { get; set; }

        public override string ToString()
        {
            return $"{AverageRate} => {TotalRateCount}";
        }
    }
}
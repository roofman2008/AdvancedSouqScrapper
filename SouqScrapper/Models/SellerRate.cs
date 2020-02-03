namespace SouqScrapper.Models
{
    public class SellerRate
    {
        public int PositiveRatePercentage { get; set; }
        public int NegativeRatePercentage { get; set; }
        public int TotalRating { get; set; }

        public override string ToString()
        {
            return $"{PositiveRatePercentage}-{NegativeRatePercentage} => {TotalRating}";
        }
    }
}
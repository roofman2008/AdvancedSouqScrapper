using System;

namespace SouqScrapper.Models
{
    public class SellerReview
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comment { get; set; }
        public string Username { get; set; }
        public Product Product { get; set; }
        public FeedbackType FeedbackType { get; set; }
        public Seller Seller { get; set; }

        public override string ToString()
        {
            return $"{Username}: {Comment}";
        }
    }
}
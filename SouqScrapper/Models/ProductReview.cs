using System;

namespace SouqScrapper.Models
{
    public class ProductReview
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comment { get; set; }
        public string Username { get; set; }
        public bool IsPurchased { get; set; }
        public Product Product { get; set; }

        public override string ToString()
        {
            return $"{Username}: {Comment}";
        }
    }
}
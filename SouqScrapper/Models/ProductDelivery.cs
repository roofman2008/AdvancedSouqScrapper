using System;

namespace SouqScrapper.Models
{
    public class ProductDelivery
    {
        public Guid Id { get; set; }
        public City City { get; set; }
        public bool CanDeliver { get; set; }
        public int? EstimatedDays { get; set; }
        public Product Product { get; set; }

        public override string ToString()
        {
            return $"[{City}]:{EstimatedDays}";
        }
    }
}
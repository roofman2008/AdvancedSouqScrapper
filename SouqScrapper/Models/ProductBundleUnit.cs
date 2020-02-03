using System;

namespace SouqScrapper.Models
{
    public class ProductBundleUnit
    {
        public Guid Id { get; set; }
        public ProductBundle Bundle { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }
        public double CurrentPrice { get; set; }
        public double AbsolutePrice { get; set; }
    }
}
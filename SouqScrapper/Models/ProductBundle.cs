using System;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class ProductBundle
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string BundleId { get; set; }
        public double CurrentPrice { get; set; }
        public double Discount { get; set; }
        public double AbsolutePrice { get; set; }
        public string Currency { get; set; }
        public Product Product { get; set; }
        public int Quantitiy { get; set; }
        public ICollection<ProductBundleUnit> BundleUnits { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
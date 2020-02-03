using System;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string UnitId { get; set; }
        public string ProductId { get; set; }
        public string Ean { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Manufacturer { get; set; }
        public Category Category { get; set; }
        public double CurrentPrice { get; set; }
        public bool HasDiscount { get; set; }
        public double? AbsolutePrice { get; set; }
        public double? DiscountSaving { get; set; }
        public int? DiscountPercentage { get; set; }
        public string Currency { get; set; }
        public string Condition { get; set; }
        public Seller Seller { get; set; }
        public string SellerNote { get; set; }
        public bool HasRating { get; set; }
        public ProductRate Rate { get; set; }
        public int ReviewsCount { get; set; }
        public ICollection<ProductReview> Reviews { get; set; }
        public bool IsSouqFulfiled { get; set; }
        public bool IsFreeShipping { get; set; }
        public ProductAvailability Availability { get; set; }
        public int? StockCount { get; set; }
        public ICollection<ProductAttribute> Attributes { get; set; }
        public ICollection<Hyperlink> Hyperlinks { get; set; }
        public bool IsBundled { get; set; }
        public ICollection<ProductBundle> Bundles { get; set; }
        public ICollection<ProductDelivery> Deliveries { get; set; }
        public bool IsFaulty { get; set; }
        public ICollection<Product> ProductConfigurations { get; set; }
        public Product ProductParentConfiguration { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
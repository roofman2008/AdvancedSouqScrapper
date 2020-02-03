using System;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class Seller
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string UserCode { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime JoinDate { get; set; }
        public float AverageRate { get; set; }
        public SellerRate LastYearRate { get; set; }
        public SellerRate LastQuarterRate { get; set; }
        public SellerRate LastMonthRate { get; set; }
        public ICollection<SellerReview> Reviews { get; set; }
        public ICollection<Product> Products { get; set; }
        public bool IsFaulty { get; set; }
        public bool HasNoData { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
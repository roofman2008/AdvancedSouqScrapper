using System;
using System.Collections;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Query { get; set; }
        public string Url { get; set; }
        public ICollection<CategoryGroup> CategoryGroups { get; set; }
        public ICollection<Product> Products { get; set; }
        public int ExpectedProductCount { get; set; }
        public int ActualProductCount { get; set; }
        public bool HasRedirection { get; set; }
        public string RedirectQuery { get; set; }
        public string RedirectUrl { get; set; }
        public bool IsFaulty { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
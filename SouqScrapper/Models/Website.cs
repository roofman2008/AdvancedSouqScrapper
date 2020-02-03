using System;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class Website
    {
        public Guid Id { get; set; }
        public DateTime ScrapeTimestamp { get; set; }
        public string ScrapperVersion { get; set; }
        public ICollection<CategoryGroup> CategoryGroups { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<Seller> Sellers { get; set; }
        public ICollection<City> Cities { get; set; }
        public ICollection<ProductAttributeDefinition> AttributeDefinitions { get; set; }
    }
}
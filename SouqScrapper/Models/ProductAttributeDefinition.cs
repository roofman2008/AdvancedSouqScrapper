using System;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class ProductAttributeDefinition
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductAttributeValue> Values { get; set; }
        public ICollection<ProductAttribute> Attributes { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
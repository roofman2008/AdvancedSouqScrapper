using System;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class ProductAttributeValue
    {
        public Guid Id { get; set; }
        public ProductAttrbuteValueType Type { get; set; }
        public string Value { get; set; }
        public ProductAttributeDefinition ProductAttributeDefinition { get; set; }
        public ICollection<ProductAttribute> Attribute { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
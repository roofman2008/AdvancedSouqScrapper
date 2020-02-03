using System;

namespace SouqScrapper.Models
{
    public class ProductAttribute
    {
        public ProductAttributeDefinition Definition { get; set; }
        public ProductAttributeValue Value { get; set; }
        public bool IsConfiguration { get; set; }

        public override string ToString()
        {
            return $"[{Definition.Name}]:{Value.Value}";
        }
    }
}
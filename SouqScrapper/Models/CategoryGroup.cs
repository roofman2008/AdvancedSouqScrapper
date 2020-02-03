using System;
using System.Collections.Generic;

namespace SouqScrapper.Models
{
    public class CategoryGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public CategoryGroup Parent { get; set; }
        public ICollection<Category> Categories { get; set; }
        public ICollection<CategoryGroup> CategoryGroups { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
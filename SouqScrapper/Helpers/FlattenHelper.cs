using System.Collections.Generic;
using System.Linq;
using System.Text;
using SouqScrapper.Models;

namespace SouqScrapper.Helpers
{
    public static class FlattenHelper
    {
        public static IEnumerable<Category> FlattenCategories(this CategoryGroup categoryGroup)
        {
            var categories = new List<Category>();

            if (categoryGroup.Categories.Count > 0)
                categories.AddRange(categoryGroup.Categories);

            foreach (var childCategoryGroup in categoryGroup.CategoryGroups)
            {
                if (childCategoryGroup.CategoryGroups.Count > 0)
                    categories.AddRange(FlattenCategories(childCategoryGroup));
            }

            return categories;
        }

        public static IEnumerable<Category> FlattenCategories(this IEnumerable<CategoryGroup> categoryGroups)
        {
            return categoryGroups.SelectMany(l => l.FlattenCategories()).ToList();
        }

        public static IEnumerable<Product> FlattenProducts(this IEnumerable<Category> categories)
        {
            return categories.Where(l=>l.Products != null).SelectMany(l => l.Products).ToList();
        }

        public static string FlattenString(this IEnumerable<string> strs)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var str in strs)
            {
                sb.Append(str);
                sb.Append(",");
            }

            return sb.ToString(0, sb.Length - 1);
        }
    }
}
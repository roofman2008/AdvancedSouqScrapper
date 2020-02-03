using System;
using System.IO;
using System.Linq;
using System.Text;
using SouqScrapper.Helpers;
using SouqScrapper.Models;

namespace SouqScrapper.Stores
{
    public class ExcelStore
    {
        public void Store(Website website)
        {
            var products = website.Categories.FlattenProducts();

            StringBuilder sb = new StringBuilder();
            var properties = products.First().GetType().GetProperties();

            foreach (var property in properties)
            {
                sb.Append(property.Name);
                sb.Append("|");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.AppendLine();

            foreach (var product in products)
            {
                foreach (var property in properties)
                {
                    object value = property.GetValue(product);
                    sb.Append(value);
                    sb.Append("|");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.AppendLine();
            }

            File.WriteAllText("output.csv", sb.ToString());
        }
    }
}
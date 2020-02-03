using System.Collections.Generic;
using Newtonsoft.Json;

namespace SouqScrapper.ApiModels
{
    public class SouqBundle
    {
        public int id_bundle { get; set; }
        public string title { get; set; }
        public string badge_title { get; set; }
        public double price { get; set; }
        public string formatted_price { get; set; }
        public string discount { get; set; }
        public int qty { get; set; }
        public string discount_amount { get; set; }
        public string discount_amount_formatted { get; set; }
        public string bundle_type { get; set; }
        public bool? is_bogo { get; set; }
        [JsonIgnore]
        public IEnumerable<SouqBundleUnits> units { get; set; }
        public string bundle_units_original_price { get; set; }
        public long id_unit { get; set; }
        public string country_currency { get; set; }
    }

    public class SouqBundleUnits
    {
        public int idPromotionBundleUnit { get; set; }
        public long idUnit { get; set; }
        public long idItem { get; set; }
        public int qty { get; set; }
        public int isMaster { get; set; }
        public string itemEAN { get; set; }
        public string sellerName { get; set; }
        public string itemOriginalPrice { get; set; }
        public string discount { get; set; }
        public string unitTitle { get; set; }
        public string unitImg { get; set; }
        public string unitUrl { get; set; }
    }
}
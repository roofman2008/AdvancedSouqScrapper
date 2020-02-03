using System.Collections.Generic;
using Newtonsoft.Json;
using SouqScrapper.JsonHandlers;

namespace SouqScrapper.ApiModels
{
    public class SouqProductsGrid2
    {
        public bool success { get; set; }
        public SouqProductsGrid2Container jsonData { get; set; }
        public int show_more { get; set; }
        public string redirect_url { get; set; }
        public int page { get; set; }
        public int section { get; set; }
    }

    public class SouqProductsGrid2Container
    {
        public SouqProductsGrid2Metadata meta_data { get; set; }
        public List<SouqProductsGrd2Unit> units { get; set; }
    }

    public class SouqProductsGrid2Metadata
    {
        public int bShowMore { get; set; }
        public string currency { get; set; }
        public string query { get; set; }
        public int total { get; set; }
    }

    public class SouqProductsGrd2Unit
    {
        public long unit_id { get; set; }
        public long item_id { get; set; }
        [JsonConverter(typeof(ForceToArrayConverter<string>))]
        public string[] ean { get; set; }
        public string title { get; set; }
        public string image_url { get; set; }
        public string primary_link { get; set; }
        public string revisioning { get; set; }
        public bool free_shipping_eligiblity { get; set; }
        public int sales_rank { get; set; }
        public double market_price { get; set; }
        public string market_price_formatted { get; set; }
        public string manufacturer { get; set; }
        public bool is_fbs { get; set; }
        public string bundle_label { get; set; }
        public double price { get; set; }
        public dynamic discount { get; set; }
        public dynamic asin { get; set; }

        /*Under Test*/
        public bool is_ags { get; set; }
        public string price_cbt { get; set; }
        public dynamic coupon { get; set; }
        public bool is_cbt { get; set; }
        public bool isInternationalSeller { get; set; }
        public dynamic shipping { get; set; }
        public dynamic selling_points { get; set; }
    }
}
namespace SouqScrapper.ApiModels
{
    public class SouqProductBucket
    {
        public SouqProductBucket_PageData Page_Data { get; set; }
        public string url { get; set; }
    }

    public class SouqProductBucket_PageData
    {
        public string page_name { get; set; }
        public string channel_name { get; set; }
        public string products { get; set; }
        public string sold_out { get; set; }
        public string s_itemConnection { get; set; }
        public string s_remaining_qty_msg { get; set; }
        public long ItemIDs { get; set; }
        public string seller_rating { get; set; }
        public string product_price { get; set; }
        public string s_ean { get; set; }
        public string item_title { get; set; }
        public string seller_name { get; set; }
        public int item_reviews { get; set; }
        public string s_item_rating_avg { get; set; }
        public int s_item_rating_total { get; set; }
        public string price_ranges { get; set; }
        public string s_itemTitle { get; set; }
        public string s_shipping_Fees { get; set; }
        public string ifd_msg { get; set; }
        public SouqProductBucket_PageData_Product product { get; set; }
    }

    public class SouqProductBucket_PageData_Product
    {
        public string currencyCode { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public long id_item { get; set; }
        public string id_unit { get; set; }
        public string id_seller { get; set; }
        public string price { get; set; }
        public string brand { get; set; }
        public string category { get; set; }
        public string variant { get; set; }
        public string seller { get; set; }
        public int quantity { get; set; }
        public double discount { get; set; }
    }
}
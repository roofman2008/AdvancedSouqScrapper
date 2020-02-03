namespace SouqScrapper.ApiModels
{
    public class SouqDeliveryInfo
    {
        public int currentStep { get; set; }
        public string currentCity { get; set; }
        public string currentCityEn { get; set; }
        public int? ifd { get; set; }
        public bool is_ags { get; set; }
        public string currentCityISO { get; set; }
        public string period { get; set; }
        public bool sameDay { get; set; }
        public bool bNextDayAvailable { get; set; }
        public  bool bSmartDeliverHourAvailable { get; set; }
        public int? estimate_by_days { get; set; }
        public bool new_winner { get; set; }
        public dynamic ags_info { get; set; }
        public string sEstimated { get; set; }
    }
}
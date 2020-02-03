using System.Collections.Generic;

namespace SouqScrapper.ApiModels
{
    public class SouqProduct
    {
        public string body { get; set; }
        public List<SouqBundle> bundles { get; set; }
    }
}
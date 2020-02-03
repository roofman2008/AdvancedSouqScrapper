using System;

namespace SouqScrapper.Models
{
    public class Hyperlink
    {
        public Guid Id { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return Url;
        }
    }
}
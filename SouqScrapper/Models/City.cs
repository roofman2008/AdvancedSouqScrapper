using System;

namespace SouqScrapper.Models
{
    public class City
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
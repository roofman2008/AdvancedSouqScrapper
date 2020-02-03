using SouqScrapper.Models;

namespace SouqScrapper.Core
{
    public interface IParser
    {
        void Process(Website website);
    }
}
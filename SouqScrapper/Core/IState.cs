using System.Collections.Concurrent;

namespace SouqScrapper.Core
{
    public interface IState
    {
        bool IsCompleted { get; set; }
        ConcurrentQueue<object> Finished { get; set; }
    }
}
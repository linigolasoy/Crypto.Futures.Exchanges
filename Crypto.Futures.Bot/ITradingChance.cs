using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot
{

    public enum ChanceStatus
    {
        Created,
        Handling,
        Open,
        Closed,
        Canceled
    }

    /// <summary>
    /// Trading chances
    /// </summary>
    public interface ITradingChance
    {
        public ChanceStatus Status { get; internal set; }
        public string Currency { get; }
        public DateTime DateTime { get; }
        public DateTime? DateOpen { get; }
        public decimal? Profit { get; set; }
        public bool Update();
    }
}

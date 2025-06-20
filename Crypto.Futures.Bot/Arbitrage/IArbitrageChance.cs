using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{

    internal enum ArbitrageStatus
    {
        Created,
        Handling,
        Open,
        Closed,
        Canceled
    }
    
    internal interface IArbitrageChance
    {
        public IArbitrageFinder Finder { get; }

        public string Currency { get; }
        
        public ArbitrageStatus Status { get;  set; }
        public DateTime DateTime { get; }
        public DateTime? DateOpen { get; }
        public decimal Percentage { get; }
        public decimal PercentageClose { get; }
        public IArbitrageSymbolData LongData { get; }
        public IArbitrageSymbolData ShortData { get; }

        public DateTime LastLog { get; set; }
        public decimal? Profit { get; set; }
        public decimal MaxProfit { get; set; }

        public bool IsDataValid { get; }

        public Task? TradeTask { get; set; }
        public void Update(IArbitrageChance oChance);

    }
}

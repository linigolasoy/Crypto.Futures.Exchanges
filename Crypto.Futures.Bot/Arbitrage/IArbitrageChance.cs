using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{


    public interface IArbitrageChance: ITradingChance
    {
        public IArbitrageFinder Finder { get; }

        
        public decimal Percentage { get; }
        public decimal PercentageClose { get; }
        public IArbitrageSymbolData LongData { get; }
        public IArbitrageSymbolData ShortData { get; }

        public DateTime LastLog { get; set; }
        public decimal MaxProfit { get; set; }

        public bool IsDataValid { get; }

        public Task? TradeTask { get; set; }

    }
}

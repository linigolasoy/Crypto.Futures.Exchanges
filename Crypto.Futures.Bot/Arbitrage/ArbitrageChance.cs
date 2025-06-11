using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{
    internal class ArbitrageChance
    {
        private ArbitrageChance( IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort, decimal nPercent ) 
        { 
            SymbolLong = oSymbolLong;
            SymbolShort = oSymbolShort;
            Percentage = nPercent;
            DateTime = DateTime.Now;
        }

        public DateTime DateTime { get; private set; }
        public decimal Percentage { get; private set; } = 0;
        public IFuturesSymbol SymbolLong { get; }
        public IFuturesSymbol SymbolShort { get; }

        public void Update( ArbitrageChance oChance )
        {
            DateTime = oChance.DateTime;    
            Percentage = oChance.Percentage;
        }

        public static ArbitrageChance? Create( ITicker oTicker1, ITicker oTicker2 )
        {
            ITicker oTickerMin = (oTicker1.LastPrice < oTicker2.LastPrice ? oTicker1: oTicker2);
            ITicker oTickerMax = (oTicker1.LastPrice > oTicker2.LastPrice ? oTicker1 : oTicker2);
            if (oTickerMin.LastPrice == 0 || oTickerMax.LastPrice == 0) return null;
            decimal nPercent = 100.0M * (oTickerMax.LastPrice - oTickerMin.LastPrice) / oTickerMin.LastPrice;
            if (nPercent > 10.0M) return null;
            return new ArbitrageChance(oTickerMin.Symbol, oTickerMax.Symbol, nPercent);
        }

        public override string ToString()
        {
            return $"Long [{SymbolLong.ToString()}] Short [{SymbolShort.ToString()}] Percent {Percentage}";
        }
    }
}

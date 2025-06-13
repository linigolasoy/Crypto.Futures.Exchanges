using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{

    internal class ArbitrageSymbolData
    {
        public ArbitrageSymbolData( IFuturesSymbol oSymbol ) 
        { 
            Symbol = oSymbol;   
        }

        public IFuturesSymbol Symbol { get; }

        public ITrade? LastTrade { get; set; }  = null;
        public IFundingRate? FundingRate { get; set; } = null;

        public ITraderPosition? Position { get; set; } = null;
    }
    internal class ArbitrageChance
    {
        private ArbitrageChance( IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort, decimal nPercent ) 
        { 
            LongData = new ArbitrageSymbolData(oSymbolLong);
            ShortData = new ArbitrageSymbolData(oSymbolShort);
            Percentage = nPercent;
            DateTime = DateTime.Now;
            LastLog = DateTime.Now;
        }

        public DateTime DateTime { get; private set; }
        public decimal Percentage { get; internal set; } = 0;
        public ArbitrageSymbolData LongData { get; }
        public ArbitrageSymbolData ShortData { get; }

        public DateTime LastLog { get; internal set;}
        public decimal? Profit { get; internal set; } = null;
        public decimal MaxProfit { get; internal set; } = -1000;
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

        public bool Equals( ArbitrageChance oOther )
        {
            if( oOther.LongData.Symbol.Symbol != LongData.Symbol.Symbol ) return false;
            if (oOther.LongData.Symbol.Exchange.ExchangeType != LongData.Symbol.Exchange.ExchangeType) return false;
            if (oOther.ShortData.Symbol.Symbol != ShortData.Symbol.Symbol) return false;
            if (oOther.ShortData.Symbol.Exchange.ExchangeType != ShortData.Symbol.Exchange.ExchangeType) return false;
            return true;

        }


        public override string ToString()
        {
            return $"Long [{LongData.Symbol.ToString()}] Short [{ShortData.Symbol.ToString()}] Percent {Percentage}";
        }
    }
}

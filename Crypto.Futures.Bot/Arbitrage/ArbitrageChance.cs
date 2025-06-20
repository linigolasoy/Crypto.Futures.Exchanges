using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{

    internal class PArbitrageSymbolData
    {
        public PArbitrageSymbolData( IFuturesSymbol oSymbol ) 
        { 
            Symbol = oSymbol;   
        }


        public IFuturesSymbol Symbol { get; }

        public IWebsocketSymbolData? WsSymbolData { get; set; } = null;

        public ITraderPosition? Position { get; set; } = null;
    }
    internal class PArbitrageChance
    {
        private PArbitrageChance( IFuturesSymbol oSymbolLong, IFuturesSymbol oSymbolShort, decimal nPercent ) 
        { 
            LongData = new PArbitrageSymbolData(oSymbolLong);
            ShortData = new PArbitrageSymbolData(oSymbolShort);
            Percentage = nPercent;
            DateTime = DateTime.Now;
            LastLog = DateTime.Now;
        }

        public DateTime DateTime { get; private set; }
        public DateTime? DateOpen { get; internal set; } = null; 
        public decimal Percentage { get; internal set; } = 0;
        public decimal PercentageClose { get; internal set; } = 0;
        public PArbitrageSymbolData LongData { get; }
        public PArbitrageSymbolData ShortData { get; }

        public DateTime LastLog { get; internal set;}
        public decimal? Profit { get; internal set; } = null;
        public decimal MaxProfit { get; internal set; } = -1000;

        public bool IsDataValid
        {
            get
            {
                if( LongData.WsSymbolData == null ) return false;
                if (ShortData.WsSymbolData == null) return false;
                if (LongData.WsSymbolData.FundingRate == null) return false;
                if (ShortData.WsSymbolData.FundingRate == null) return false;
                if (LongData.WsSymbolData.LastPrice == null) return false;
                if (ShortData.WsSymbolData.LastPrice == null) return false;
                DateTime dNow = DateTime.Now;
                double nDiffLong  = (dNow - LongData.WsSymbolData.LastUpdate).TotalMilliseconds;
                double nDiffShort = (dNow - ShortData.WsSymbolData.LastUpdate).TotalMilliseconds;
                if( nDiffLong > 1000 || nDiffShort > 1000 ) return false;
                return true;
            }
        }

        public void Update( PArbitrageChance oChance )
        {
            DateTime = oChance.DateTime;    
            Percentage = oChance.Percentage;
        }

        public static PArbitrageChance? Create( ITicker oTicker1, ITicker oTicker2 )
        {
            ITicker oTickerMin = (oTicker1.LastPrice < oTicker2.LastPrice ? oTicker1: oTicker2);
            ITicker oTickerMax = (oTicker1.LastPrice > oTicker2.LastPrice ? oTicker1 : oTicker2);
            if (oTickerMin.LastPrice == 0 || oTickerMax.LastPrice == 0) return null;
            decimal nPercent = 100.0M * (oTickerMax.LastPrice - oTickerMin.LastPrice) / oTickerMin.LastPrice;
            if (nPercent > 10.0M) return null;
            return new PArbitrageChance(oTickerMin.Symbol, oTickerMax.Symbol, nPercent);
        }

        public bool Equals( PArbitrageChance oOther )
        {
            if( oOther.LongData.Symbol.Symbol != LongData.Symbol.Symbol ) return false;
            if (oOther.LongData.Symbol.Exchange.ExchangeType != LongData.Symbol.Exchange.ExchangeType) return false;
            if (oOther.ShortData.Symbol.Symbol != ShortData.Symbol.Symbol) return false;
            if (oOther.ShortData.Symbol.Exchange.ExchangeType != ShortData.Symbol.Exchange.ExchangeType) return false;
            return true;

        }


        public override string ToString()
        {
            decimal nPercent = Math.Round(Percentage, 2);
            return $"Long [{LongData.Symbol.ToString()}] Short [{ShortData.Symbol.ToString()}] Percent {nPercent}%";
        }
    }
}

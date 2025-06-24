using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage.Model
{
    internal class ArbitrageChanceModel : IArbitrageChance
    {
        public ArbitrageChanceModel( 
            IArbitrageFinder oFinder, 
            string strCurrency, 
            decimal nPercent,
            IWebsocketSymbolData oDataLong, IWebsocketSymbolData oDataShort ) 
        { 
            Finder = oFinder;
            Currency = strCurrency;
            DateTime = DateTime.Now;
            LongData = new ArbitrageSymbolData(this, oDataLong);
            ShortData = new ArbitrageSymbolData(this, oDataShort);
            Percentage = nPercent;
        }
        public IArbitrageFinder Finder { get; }

        public string Currency { get; }

        public ChanceStatus Status { get; set;  } = ChanceStatus.Created;
        public DateTime DateTime { get; }

        public DateTime? DateOpen { get; internal set; } = null;

        public decimal Percentage { get; internal set; } = 0;

        public decimal PercentageClose { get; internal set; } = 0;

        public IArbitrageSymbolData LongData { get; }

        public IArbitrageSymbolData ShortData { get; }

        public Task? TradeTask { get; set; } = null;

        public DateTime LastLog { get; set; } = DateTime.MinValue;

        public decimal? Profit { get; set; } = null;

        public decimal MaxProfit { get; set; } = -100;

        public bool IsDataValid
        {
            get
            {
                if (LongData.WsSymbolData == null) return false;
                if (ShortData.WsSymbolData == null) return false;
                if (LongData.WsSymbolData.FundingRate == null) return false;
                if (ShortData.WsSymbolData.FundingRate == null) return false;
                if (LongData.WsSymbolData.LastPrice == null) return false;
                if (ShortData.WsSymbolData.LastPrice == null) return false;
                if (LongData.WsSymbolData.LastOrderbookPrice == null) return false;
                if (ShortData.WsSymbolData.LastOrderbookPrice == null) return false;

                decimal nPriceBidLong = LongData.WsSymbolData.LastOrderbookPrice.BidPrice;
                decimal nPriceAskLong = LongData.WsSymbolData.LastOrderbookPrice.AskPrice;

                if (nPriceBidLong <= 0 || nPriceAskLong <= 0) return false;
                decimal nDiffLong = Math.Abs( nPriceAskLong - nPriceBidLong);
                decimal nPercentLong = nDiffLong * 100.0M / nPriceBidLong;  
                if( nPercentLong > 1.0M) return false; // 1% difference is too high 

                decimal nPriceBidShort = ShortData.WsSymbolData.LastOrderbookPrice.BidPrice;
                decimal nPriceAskShort = ShortData.WsSymbolData.LastOrderbookPrice.AskPrice;

                if (nPriceBidShort <= 0 || nPriceAskShort <= 0) return false;
                decimal nDiffShort = Math.Abs(nPriceAskShort - nPriceBidShort);
                decimal nPercentShort = nDiffShort * 100.0M / nPriceBidShort;
                if (nPercentLong > 1.0M) return false; // 1% difference is too high 

                DateTime dNow = DateTime.Now;
                double nDiffTimeLong = (dNow - LongData.Symbol.Exchange.Market.Websocket.DataManager.LastUpdate).TotalMilliseconds;
                double nDiffTimeShort = (dNow - ShortData.Symbol.Exchange.Market.Websocket.DataManager.LastUpdate).TotalMilliseconds;
                if (nDiffTimeLong > 1000 || nDiffTimeShort > 1000) return false;
                return true;
            }
        }

        public bool Update()
        {
            if( !IsDataValid) 
            {
                return false;
            }
            decimal nPriceLong  = LongData.WsSymbolData!.LastOrderbookPrice!.AskPrice;
            decimal nPriceShort = ShortData.WsSymbolData!.LastOrderbookPrice!.BidPrice;
            if( nPriceLong <= 0 || nPriceShort <= 0)
            {
                return false;
            }
            decimal nDiff = nPriceShort - nPriceLong;
            decimal nPercent = nDiff * 100.0M / nPriceLong;
            Percentage = Math.Round(nPercent, 2);
            return true;
        }

        public override string ToString()
        {
            decimal nPercent = Math.Round(Percentage, 2);
            return $"Long [{LongData.Symbol.ToString()}] Short [{ShortData.Symbol.ToString()}] Percent {nPercent}%";
        }

    }
}

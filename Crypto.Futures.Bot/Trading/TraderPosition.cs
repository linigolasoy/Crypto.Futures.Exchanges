using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Trading
{
    internal class TraderPosition : ITraderPosition
    {
        private static long m_nIdCounter = 1;
        public TraderPosition(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal nPriceOpen ) 
        { 
            Symbol = oSymbol;
            IsLong = bLong;
            Volume = nVolume;
            PriceOpen = nPriceOpen;
            PriceClose = nPriceOpen;
            Id = ++m_nIdCounter;
        }
        public IFuturesSymbol Symbol { get; }
        public long Id { get; }

        public bool IsLong { get; }

        public decimal Volume { get; }

        public decimal PriceOpen { get; }

        public decimal PriceClose { get; set; }
        public decimal ActualPrice { get; private set; } // Current price of the position, if open   

        public DateTime DateOpen { get; }

        public DateTime? DateClose { get; set; } = null;

        public decimal Profit { get; private set; } = 0;
        public bool Update()
        {
            // Get last trade
            IWebsocketSymbolData? oData = Symbol.Exchange.Market.Websocket.DataManager.GetData( Symbol );
            if (oData == null) return false;
            if( oData.LastOrderbookPrice == null ) return false;
            DateTime dNow = DateTime.Now;
            double nSeconds =( dNow - oData.LastOrderbookPrice.DateTime).TotalSeconds;
            if( nSeconds > 1.0 ) return false; // Data is not fresh enough, we need to wait for next update 
            decimal nPrice = (IsLong ? oData.LastOrderbookPrice.BidPrice : oData.LastOrderbookPrice.AskPrice);
            ActualPrice = nPrice;
            decimal nDiff = nPrice - PriceOpen;
            if (!IsLong) nDiff *= -1.0M;
            decimal nFeesOpen  = (IsLong ? Symbol.FeeTaker : Symbol.FeeMaker) * PriceOpen * Volume;
            decimal nFeesClose = (IsLong ? Symbol.FeeMaker : Symbol.FeeTaker) * nPrice * Volume;
            PriceClose = nPrice;
            Profit = nDiff * Volume - nFeesOpen- nFeesClose;
            return true;
        }
    }
}

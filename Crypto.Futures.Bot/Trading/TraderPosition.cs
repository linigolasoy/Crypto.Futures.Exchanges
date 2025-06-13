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
        public TraderPosition(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal nPriceOpen ) 
        { 
            Symbol = oSymbol;
            IsLong = bLong;
            Volume = nVolume;
            PriceOpen = nPriceOpen;
            PriceClose = nPriceOpen;
        }
        public IFuturesSymbol Symbol { get; }

        public bool IsLong { get; }

        public decimal Volume { get; }

        public decimal PriceOpen { get; }

        public decimal PriceClose { get; set; }

        public DateTime DateOpen { get; }

        public DateTime? DateClose { get; set; } = null;

        public decimal Profit { get; private set; } = 0;
        public void Update()
        {
            // Get last trade
            IWebsocketSymbolData? oData = Symbol.Exchange.Market.Websocket.DataManager.GetData( Symbol );
            if (oData == null) return;
            if( oData.LastTrade == null ) return;
            decimal nPrice = oData.LastTrade.Price;
            decimal nDiff = nPrice - PriceOpen;
            if (!IsLong) nDiff *= -1.0M;
            PriceClose = nPrice;
            Profit = nDiff * Volume;
        }
    }
}

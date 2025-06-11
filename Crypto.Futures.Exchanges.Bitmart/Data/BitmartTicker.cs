using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{
    internal class BitmartTicker: ITicker
    {
        public BitmartTicker( IFuturesSymbol oSymbol, BitmartSymbolJson oJson) 
        { 
            Symbol = oSymbol;
            DateTime = DateTime.Now;

            LastPrice = decimal.Parse( oJson.LastPrice, CultureInfo.InvariantCulture);
            AskPrice = LastPrice;
            BidPrice = LastPrice;
            AskVolume = 0;
            BidVolume = 0;
        }
        public DateTime DateTime { get; private set; }

        public decimal LastPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidVolume { get; private set; }
        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is ITicker)) return;
            ITicker oTicker = (ITicker)oMessage;
            DateTime = oTicker.DateTime;
            AskPrice = oTicker.AskPrice;
            BidPrice = oTicker.BidPrice;
            AskVolume = oTicker.AskVolume;
            BidVolume = oTicker.BidVolume;
            LastPrice = oTicker.LastPrice;

        }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        public static ITicker? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if ( oToken == null ) return null;
            BitmartSymbolJson? oJson = oToken.ToObject<BitmartSymbolJson>(); 
            if ( oJson == null ) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if ( oSymbol == null ) return null;
            return new BitmartTicker(oSymbol, oJson);
        }
    }
}

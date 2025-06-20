using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Data
{



    internal class BlofinOrderbookPrice : IOrderbookPrice
    {

        public BlofinOrderbookPrice( ITicker oTicker )
        {
            DateTime = oTicker.DateTime;
            AskPrice = oTicker.AskPrice;
            AskVolume = oTicker.AskVolume;
            BidPrice = oTicker.BidPrice;
            BidVolume = oTicker.BidVolume;
            Symbol = oTicker.Symbol;
        }

        public DateTime DateTime { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal BidVolume { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.OrderbookPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IOrderbookPrice)) return;
            IOrderbookPrice oPrice = (IOrderbookPrice)oMessage; 
            DateTime = oPrice.DateTime;
            AskPrice = oPrice.AskPrice;
            AskVolume = oPrice.AskVolume;
            BidPrice = oPrice.BidPrice;
        }

    }
}

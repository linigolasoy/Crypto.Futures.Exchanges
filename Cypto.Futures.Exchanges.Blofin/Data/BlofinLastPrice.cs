using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Data
{
    internal class BlofinLastPrice : ILastPrice
    {
        public BlofinLastPrice( ITicker oTicker ) 
        { 
            Symbol = oTicker.Symbol;
            DateTime = oTicker.DateTime;
            Price = oTicker.LastPrice;
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if( !( oMessage is ILastPrice )) return;
            ILastPrice oPrice = ( ILastPrice )oMessage; 
            DateTime = oPrice.DateTime;
            Price = oPrice.Price;
        }
    }
}

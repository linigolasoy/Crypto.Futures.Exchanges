using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{
    internal class MexcLastPrice : ILastPrice
    {
        public MexcLastPrice(ITicker oTicker) 
        { 
            Price = oTicker.LastPrice;
            Symbol = oTicker.Symbol;
            DateTime = oTicker.DateTime;
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if(!(oMessage is ILastPrice)) return;
            ILastPrice oPrice = (ILastPrice)oMessage;
            Price = oPrice.Price;
            DateTime = oPrice.DateTime;
        }
    }
}

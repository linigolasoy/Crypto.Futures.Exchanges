using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toobit.Net.Objects.Models;

namespace Crypto.Futures.Exchanges.Toobit.Data
{
    internal class ToobitLastPrice : ILastPrice
    {
        public ToobitLastPrice(IFuturesSymbol oSymbol, ToobitTickerUpdate oUpdate) 
        { 
            Symbol = oSymbol;
            DateTime = DateTime.Now;
            Price = (oUpdate.LastPrice == null ? 0 : oUpdate.LastPrice.Value);
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is ILastPrice)) return;
            ILastPrice oPrice = (ILastPrice)oMessage;
            if( oPrice.Price <= 0 ) return;
            DateTime = oPrice.DateTime;
            Price = oPrice.Price;
        }
    }
}

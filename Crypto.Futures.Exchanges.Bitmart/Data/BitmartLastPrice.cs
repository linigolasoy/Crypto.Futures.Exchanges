using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{
    internal class BitmartLastPrice : ILastPrice
    {

        public BitmartLastPrice(IFuturesSymbol oSymbol, BitMartFuturesTickerUpdate oUpdate, DateTime dDate)
        {
            Symbol = oSymbol;
            Price = oUpdate.LastPrice;
            DateTime = dDate.ToLocalTime();
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage)
        {
            if ( !(oMessage is ILastPrice) ) return;
            var oLastPrice = (ILastPrice)oMessage;
            Price = oLastPrice.Price;
            DateTime = oLastPrice.DateTime;
        }
    }
}

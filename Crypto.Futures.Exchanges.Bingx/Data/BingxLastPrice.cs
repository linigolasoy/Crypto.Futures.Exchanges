using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Data
{


    internal class BingxLastPrice : ILastPrice
    {
        public BingxLastPrice(IFuturesSymbol oSymbol, BingXFuturesTickerUpdate oJson)
        {
            Symbol = oSymbol;
            DateTime = oJson.EventTime.ToLocalTime();   
            Price = oJson.LastPrice;
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if(!(oMessage is ILastPrice)) return;
            ILastPrice oLastPrice = (ILastPrice)oMessage;
            Price = oLastPrice.Price;
            DateTime = oLastPrice.DateTime;
        }

    }
}

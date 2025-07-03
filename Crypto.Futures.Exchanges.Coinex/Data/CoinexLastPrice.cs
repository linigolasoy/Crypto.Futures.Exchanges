using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Data
{
    
    internal class CoinexLastPrice : ILastPrice
    {

        public CoinexLastPrice(IFuturesSymbol oSymbol, CoinExFuturesTickerUpdate oJson, DateTime? dTime)
        {
            Symbol = oSymbol;
            Price = oJson.LastPrice;
            DateTime = (dTime == null ? DateTime.Now : dTime.Value.ToLocalTime());
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is ILastPrice)) return;
            ILastPrice oLastPrice = (ILastPrice)oMessage;
            Price = oLastPrice.Price;
            DateTime = oLastPrice.DateTime;
        }
    }
}

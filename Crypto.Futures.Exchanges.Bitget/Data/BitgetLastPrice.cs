using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    
    internal class BitgetLastPrice : ILastPrice
    {
        public BitgetLastPrice(IFuturesSymbol oSymbol, BitgetFuturesTickerUpdate oJson)
        {
            Symbol = oSymbol;
            Price = oJson.LastPrice;
            DateTime = oJson.Timestamp.ToLocalTime();
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage)
        {
            if(!(oMessage is ILastPrice)) return;
            ILastPrice oLastPrice = (ILastPrice)oMessage;
            Price = oLastPrice.Price;
            DateTime = oLastPrice.DateTime;
        }
    }
}

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
    internal class BingxLastPriceJson
    {
        [JsonProperty("e")]
        public string EventType { get; set; } = string.Empty; // Event type, e.g., "lastPrice"
        [JsonProperty("E")]
        public string EventTime { get; set; } = string.Empty; // Event time in milliseconds since epoch
        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty; //
        [JsonProperty("c")]
        public string LastPrice { get; set; } = string.Empty; //
    }
    internal class BingxLastPrice : ILastPrice
    {
        public BingxLastPrice(IFuturesSymbol oSymbol, BingxLastPriceJson oJson)
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.EventTime, true);
            Price = decimal.Parse(oJson.LastPrice, CultureInfo.InvariantCulture);
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

        public static IWebsocketMessage[]? Parse( IFuturesExchange oExchange, string strSymbol, JToken? oToken )
        {
            if (oToken == null) return null;
            BingxLastPriceJson? oJson = oToken.ToObject<BingxLastPriceJson>();
            if (oJson == null) return null;
            if( strSymbol != oJson.Symbol) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(strSymbol);
            if (oSymbol == null) return null; // Symbol not found
            return new IWebsocketMessage[] { new BingxLastPrice(oSymbol, oJson) };  
        }
    }
}

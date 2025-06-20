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
using System.Transactions;

namespace Crypto.Futures.Exchanges.Bingx.Data
{

    internal class BingxOrderbookPriceJson
    {
        [JsonProperty("e")]
        public string Eventtype { get; set; } = string.Empty; // Event type, e.g., "bookTicker" 
        [JsonProperty("u")]
        public string UpdateId { get; set; } = string.Empty; // Update ID, e.g., 123456789
        [JsonProperty("E")]
        public string EventTime { get; set; } = string.Empty; // Event time in milliseconds since epoch
        [JsonProperty("T")]
        public string TransactionTime { get; set; } = string.Empty; // Transaction time in milliseconds since epoch
        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty; //
        [JsonProperty("b")]
        public string BidPrice { get; set; } = string.Empty; // 
        [JsonProperty("B")]
        public string BidVolume { get; set; } = string.Empty; // 
        [JsonProperty("a")]
        public string AskPrice { get; set; } = string.Empty; //
        [JsonProperty("A")]
        public string AskVolume { get; set; } = string.Empty; //
    }
    internal class BingxOrderbookPrice : IOrderbookPrice
    {

        public BingxOrderbookPrice(IFuturesSymbol oSymbol, BingxOrderbookPriceJson oJson)
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.EventTime, true);
            AskPrice = decimal.Parse(oJson.AskPrice, CultureInfo.InvariantCulture);
            AskVolume = decimal.Parse(oJson.AskVolume, CultureInfo.InvariantCulture);
            BidPrice = decimal.Parse(oJson.BidPrice, CultureInfo.InvariantCulture);
            BidVolume = decimal.Parse(oJson.BidVolume, CultureInfo.InvariantCulture);
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
            BidVolume = oPrice.BidVolume;
        }

        public static IWebsocketMessage[]? Parse(IFuturesExchange oExchange, string strSymbol, JToken? oToken)
        {
            if (oToken == null) return null;
            BingxOrderbookPriceJson? oJson = oToken.ToObject<BingxOrderbookPriceJson>();
            if(oJson == null) return null;  
            string strSymbolJson = oJson.Symbol;
            if (strSymbolJson != strSymbol) return null; // Symbol mismatch
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(strSymbol);
            if (oSymbol == null) return null; // Symbol not found in exchange
            return new IWebsocketMessage[] { new BingxOrderbookPrice(oSymbol, oJson) }; // Return the parsed order book price
        }
    }
}

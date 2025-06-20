using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartTickerJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("last_price")]
        public string LastPrice { get; set; } = string.Empty;
        [JsonProperty("volume_24")]
        public string Volume24 { get; set; } = string.Empty;
        [JsonProperty("range")]
        public string Range { get; set; } = string.Empty; // Up or Down
        [JsonProperty("mark_price")]
        public string MarkPrice { get; set; } = string.Empty; // Mark Price
        [JsonProperty("index_price")]
        public string IndexPrice { get; set; } = string.Empty; // Index Price
        [JsonProperty("ask_price")]
        public string AskPrice { get; set; } = string.Empty; // Sell depths first price
        [JsonProperty("ask_vol")]
        public string AskVolume { get; set; } = string.Empty; // Sell depths first vol
        [JsonProperty("bid_price")]
        public string BidPrice { get; set; } = string.Empty; // Buy depths first price
        [JsonProperty("bid_vol")]
        public string BidVolume { get; set; } = string.Empty; // Buy depths first vol
    }
    internal class BitmartTicker : ITicker
    {
        public BitmartTicker(IFuturesSymbol oSymbol, BitmartSymbolJson oJson)
        {
            Symbol = oSymbol;
            DateTime = DateTime.Now;

            LastPrice = decimal.Parse(oJson.LastPrice, CultureInfo.InvariantCulture);
            AskPrice = LastPrice;
            BidPrice = LastPrice;
            AskVolume = 0;
            BidVolume = 0;
        }
        public DateTime DateTime { get; private set; }

        public decimal LastPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidVolume { get; private set; }
        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is ITicker)) return;
            ITicker oTicker = (ITicker)oMessage;
            DateTime = oTicker.DateTime;
            AskPrice = oTicker.AskPrice;
            BidPrice = oTicker.BidPrice;
            AskVolume = oTicker.AskVolume;
            BidVolume = oTicker.BidVolume;
            LastPrice = oTicker.LastPrice;

        }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        public static ITicker? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            BitmartSymbolJson? oJson = oToken.ToObject<BitmartSymbolJson>();
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;
            return new BitmartTicker(oSymbol, oJson);
        }

        public static IWebsocketMessage[]? ParseWs(IFuturesSymbol oSymbol, JToken? oToken)
        {
            if (oToken == null) return null;
            BitmartTickerJson? oTicker = oToken.ToObject<BitmartTickerJson>();
            if (oTicker == null) return null;
            if (oSymbol.Symbol != oTicker.Symbol) return null;
            return new IWebsocketMessage[]
            {
                new BitmartLastPrice(oSymbol, oTicker)
            };
        }
    }
}

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

    internal class BingxTickerJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("priceChange")]
        public string PriceChange { get; set; } = string.Empty;
        [JsonProperty("priceChangePercent")]
        public string PriceChangePercent { get; set; } = string.Empty;
        [JsonProperty("lastPrice")]
        public string LastPrice { get; set; } = string.Empty;
        [JsonProperty("lastQty")]
        public string LastQty { get; set; } = string.Empty;
        [JsonProperty("highPrice")]
        public string HighPrice { get; set; } = string.Empty;
        [JsonProperty("lowPrice")]
        public string LowPrice { get; set; } = string.Empty;
        [JsonProperty("volume")]
        public string Volume { get; set; } = string.Empty;
        [JsonProperty("quoteVolume")]
        public string QuoteVolume { get; set; } = string.Empty;
        [JsonProperty("openPrice")]
        public string OpenPrice { get; set; } = string.Empty;
        [JsonProperty("openTime")]
        public long OpenTime { get; set; } = 0;
        [JsonProperty("closeTime")]
        public long CloseTime { get; set; } = 0;
        [JsonProperty("bidPrice")]
        public decimal BidPrice { get; set; } = 0;
        [JsonProperty("bidQty")]
        public decimal BidQty { get; set; } = 0;
        [JsonProperty("askPrice")]
        public decimal AskPrice { get; set; } = 0;
        [JsonProperty("askQty")]
        public decimal AskQty { get; set; } = 0;
    }
    internal class BingxTicker: ITicker
    {
        public BingxTicker(IFuturesSymbol oSymbol, BingxTickerJson oJson )
        {
            Symbol = oSymbol;
            DateTime = DateTime.Now;
            LastPrice = decimal.Parse(oJson.LastPrice, CultureInfo.InvariantCulture);
            AskPrice = oJson.AskPrice;
            AskVolume = oJson.AskQty * oSymbol.ContractSize;
            BidPrice = oJson.BidPrice;
            BidVolume = oJson.BidQty * oSymbol.ContractSize;
        }

        public DateTime DateTime { get; }

        public decimal LastPrice { get; }

        public decimal AskPrice { get; }

        public decimal BidPrice { get; }

        public decimal AskVolume { get; }

        public decimal BidVolume { get; }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        public static ITicker? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            BingxTickerJson? oJson = oToken.ToObject<BingxTickerJson>();    
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;

            return new BingxTicker(oSymbol, oJson);
        }
    }
}

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

namespace Crypto.Futures.Exchanges.Bitunix.Data
{

    internal class BitunixTickerJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("markPrice")]
        public string MarkPrice { get; set; } = string.Empty;
        [JsonProperty("lastPrice")]
        public string LastPrice { get; set; } = string.Empty;
        [JsonProperty("open")]
        public string Open { get; set; } = string.Empty;
        [JsonProperty("last")]
        public string Last { get; set; } = string.Empty;
        [JsonProperty("quoteVol")]
        public string QuoteVol { get; set; } = string.Empty;
        [JsonProperty("baseVol")]
        public string BaseVol { get; set; } = string.Empty;
        [JsonProperty("high")]
        public string High { get; set; } = string.Empty;
        [JsonProperty("low")]
        public string Low { get; set; } = string.Empty;

    }
    internal class BitunixTicker: ITicker
    {
        public BitunixTicker( IFuturesSymbol oSymbol, BitunixTickerJson oJson)
        {
            Symbol = oSymbol;
            DateTime = DateTime.Now;
            LastPrice = decimal.Parse(oJson.LastPrice, CultureInfo.InvariantCulture);
        }
        public DateTime DateTime { get; }

        public decimal LastPrice { get; }

        public decimal AskPrice { get; } = decimal.Zero;

        public decimal BidPrice { get; } = decimal.Zero;

        public decimal AskVolume { get; } = decimal.Zero;

        public decimal BidVolume { get; } = decimal.Zero;

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        public static ITicker? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if(oToken == null) return null;
            BitunixTickerJson? oTickerJson = oToken.ToObject<BitunixTickerJson>();
            if (oTickerJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oTickerJson.Symbol);
            if (oSymbol == null) return null;
            return new BitunixTicker(oSymbol, oTickerJson);
        }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}

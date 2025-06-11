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
using System.Xml.Linq;

namespace Crypto.Futures.Exchanges.Coinex.Data
{
    internal class CoinexTickerJson
    {
        [JsonProperty("market")]
        public string Symbol { get; set; } = string.Empty;  
        [JsonProperty("last")]
        public string Last { get; set; } = string.Empty;
        [JsonProperty("open")]
        public string Open { get; set; } = string.Empty;
        [JsonProperty("close")]
        public string Close { get; set; } = string.Empty;
        [JsonProperty("high")]
        public string High { get; set; } = string.Empty;
        [JsonProperty("low")]
        public string Low { get; set; } = string.Empty;
        [JsonProperty("volume")]
        public string Volume { get; set; } = string.Empty;
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
        [JsonProperty("volume_sell")]
        public string VolumeSell { get; set; } = string.Empty;
        [JsonProperty("volume_buy")]
        public string VolumeBuy { get; set; } = string.Empty;
        [JsonProperty("index_price")]
        public string IndexPrice { get; set; } = string.Empty;
        [JsonProperty("mark_price")]
        public string MarkPrice { get; set; } = string.Empty;
    }

    internal class CoinexTicker: ITicker
    {
        public CoinexTicker(IFuturesSymbol oSymbol, CoinexTickerJson oJson )
        {
            Symbol = oSymbol;
            DateTime = DateTime.Now;
            LastPrice = decimal.Parse(oJson.Last, CultureInfo.InvariantCulture);
            AskPrice = LastPrice;
            BidPrice = LastPrice;
            AskVolume = 0;
            BidVolume = 0;
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
            if( oToken == null ) return null;
            CoinexTickerJson? oJson = oToken.ToObject<CoinexTickerJson>();
            if( oJson == null ) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if( oSymbol == null ) return null;
            return new CoinexTicker(oSymbol, oJson);
        }
    }
}

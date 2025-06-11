using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bitget.Data
{

    internal class BitgetTickerJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("lastPr")]
        public string LastPrice { get; set; } = string.Empty;
        [JsonProperty("askPr")]
        public string AskPrice { get; set; } = string.Empty;
        [JsonProperty("bidPr")]
        public string BidPrice { get; set; } = string.Empty;
        [JsonProperty("bidSz")]
        public string BidSize { get; set; } = string.Empty;
        [JsonProperty("askSz")]
        public string AskSize { get; set; } = string.Empty;
        [JsonProperty("high24h")]
        public string High24h { get; set; } = string.Empty;
        [JsonProperty("low24h")]
        public string Low24h { get; set; } = string.Empty;
        [JsonProperty("ts")]
        public string Timestamp { get; set; } = string.Empty;
        [JsonProperty("change24h")]
        public string Change24h { get; set; } = string.Empty;
        [JsonProperty("baseVolume")]
        public string BaseVolume { get; set; } = string.Empty;
        [JsonProperty("quoteVolume")]
        public string QuoteVolume { get; set; } = string.Empty;
        [JsonProperty("usdtVolume")]
        public string UsdtVolume { get; set; } = string.Empty;
        [JsonProperty("openUtc")]
        public string OpenUtc { get; set; } = string.Empty;
        [JsonProperty("changeUtc24h")]
        public string ChangeUtc24h { get; set; } = string.Empty;
        [JsonProperty("indexPrice")]
        public string IndexPrice { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public string FundingRate { get; set; } = string.Empty;
    }
    internal class BitgetTicker: ITicker
    {
        public BitgetTicker(IFuturesSymbol oSymbol, BitgetTickerJson oJson) 
        { 
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp, true);
            BidPrice = decimal.Parse(oJson.BidPrice, CultureInfo.InvariantCulture);
            AskPrice = decimal.Parse(oJson.AskPrice, CultureInfo.InvariantCulture);
            AskVolume = decimal.Parse(oJson.AskSize, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
            BidVolume = decimal.Parse(oJson.BidSize, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
            LastPrice = decimal.Parse(oJson.LastPrice, CultureInfo.InvariantCulture);

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
            BitgetTickerJson? oJson = oToken.ToObject<BitgetTickerJson>();  
            if( oJson == null ) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if( oSymbol == null ) return null;

            return new BitgetTicker( oSymbol, oJson );  
        }
    }
}

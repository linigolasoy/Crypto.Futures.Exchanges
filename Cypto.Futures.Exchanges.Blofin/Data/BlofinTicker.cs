using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Crypto.Futures.Exchanges.WebsocketModel;
using System.Globalization;

namespace Crypto.Futures.Exchanges.Blofin.Data
{

    internal class BlofinTickerJson
    {
        [JsonProperty("instId")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("last")] // Last traded price
        public string Last { get; set; } = string.Empty;
        [JsonProperty("lastSize")] // Last traded size
        public string LastSize { get; set; } = string.Empty;
        [JsonProperty("askPrice")]
        public string AskPrice { get; set; } = string.Empty;
        [JsonProperty("askSize")]
        public string AskSize { get; set; } = string.Empty;
        [JsonProperty("bidPrice")]
        public string BidPrice { get; set; } = string.Empty;
        [JsonProperty("bidSize")]
        public string BidSize { get; set; } = string.Empty;
        [JsonProperty("high24h")]
        public string High24h { get; set; } = string.Empty;
        [JsonProperty("open24h")]
        public string Open24h { get; set; } = string.Empty;
        [JsonProperty("low24h")]
        public string Low24h { get; set; } = string.Empty;
        [JsonProperty("volCurrency24h")]
        public string VolCurrency24h { get; set; } = string.Empty;
        [JsonProperty("vol24h")]
        public string Vol24h { get; set; } = string.Empty;
        [JsonProperty("ts")]
        public string Timestamp { get; set; } = string.Empty;
    }
    internal class BlofinTicker: ITicker
    {
        public BlofinTicker(IFuturesSymbol oSymbol, BlofinTickerJson oJson )
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp, true);
            LastPrice = decimal.Parse(oJson.Last, CultureInfo.InvariantCulture);
            AskPrice = decimal.Parse(oJson.AskPrice, CultureInfo.InvariantCulture);
            AskVolume = decimal.Parse(oJson.AskSize, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
            BidPrice = decimal.Parse(oJson.BidPrice, CultureInfo.InvariantCulture);
            BidVolume = decimal.Parse(oJson.BidSize, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
        }
        public DateTime DateTime { get; private set; }

        public decimal LastPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidVolume { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        
        public void Update(IWebsocketMessageBase oMessage)
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

        public static ITicker[]? ParseAll( IFuturesExchange oExchange, JToken? oToken )
        {
            if( oToken == null ) return null;

            BlofinTickerJson[]? aJson = oToken.ToObject<BlofinTickerJson[]>();
            if( aJson == null ) return null;
            List<ITicker> aResult = new List<ITicker>();
            foreach (var oJson in aJson)
            {
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
                if (oSymbol == null) continue;
                aResult.Add(new BlofinTicker(oSymbol, oJson));
            }
            return aResult.ToArray();
        }

        public static IWebsocketMessage[]? ParseWs(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            if( !( oToken is JArray) ) return null; 
            JArray oArray = (JArray)oToken; 
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            foreach( var oItem in oArray )
            {
                if( !(oItem is JObject) ) continue;
                BlofinTickerJson? oJson = oToken.ToObject<BlofinTickerJson>();
                if (oJson == null) continue;
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
                if (oSymbol == null) continue;
                ITicker? oTicker = new BlofinTicker(oSymbol, oJson);
                if ( oTicker == null ) continue;
                aResult.Add( new BlofinOrderbookPrice(oTicker) );
                aResult.Add(new BlofinLastPrice(oTicker));
            }
            return aResult.ToArray();
        }

    }
}

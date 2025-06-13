using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Ws
{

    internal enum ChannelType
    {
        ticker,
        trade
    }

    internal class BitgetMessage
    {
        [JsonProperty("action")]  
        public string? Action { get; set; } = null;
        [JsonProperty("event")]
        public string? Event { get; set; } = null;
        [JsonProperty("arg")]
        public BitgetSubscriptionChannelJson? Argument { get; set; } = null;
        [JsonProperty("code")]
        public string? Cpde { get; set; } = null;
        [JsonProperty("msg")]
        public string? Message { get; set; } = null;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;
    }

    internal class BitgetSubscriptionChannelJson
    {
        private const string USDT_FUTURES = "USDT-FUTURES";

        public BitgetSubscriptionChannelJson(ChannelType eType, IFuturesSymbol oSymbol)  
        { 
            Channel = eType.ToString();
            Symbol = oSymbol.Symbol;
        }

        public BitgetSubscriptionChannelJson() { }   

        [JsonProperty("instType")]
        public string InstrumentType { get; set; } = USDT_FUTURES;
        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;

        [JsonProperty("instId")]
        public string Symbol { get; set; } = string.Empty;

    }

    internal class BitgetSubscriptionJson
    {
        public BitgetSubscriptionJson( ChannelType eType, bool bSubscribe, IFuturesSymbol[] aSymbols )
        {
            Option = (bSubscribe ? "subscribe" : "unsubscribe");
            List<BitgetSubscriptionChannelJson> aChannels = new List<BitgetSubscriptionChannelJson>();  
            foreach( var oSymbol in aSymbols )
            {
                aChannels.Add(new BitgetSubscriptionChannelJson(eType, oSymbol));
            }
            Arguments = aChannels.ToArray();    
        }

        [JsonProperty("op")]
        public string Option { get; }
        [JsonProperty("args")]
        public BitgetSubscriptionChannelJson[] Arguments { get; }
    }
}

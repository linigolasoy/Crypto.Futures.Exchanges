using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Ws
{


    internal class BitmartMessage
    {
        [JsonProperty("action")]
        public string? Action { get; set; } = null;
        [JsonProperty("group")]
        public string? Group { get; set; } = null;
        [JsonProperty("success")]
        public bool? Success { get; set; } = null;
        [JsonProperty("request")]
        public JToken? Request { get; set; } = null;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;
    }
    internal class BitmartSubscription
    {
        public const string SUBSCRIBE = "subscribe";
        public const string UNSUBSCRIBE = "unsubscribe";

        public const string CHANNEL_FUNDING = "futures/fundingRate";
        // public const string CHANNEL_TRADE = "futures/trade";
        public const string CHANNEL_ORDERBOOK = "futures/bookticker";
        public const string CHANNEL_TICKER = "futures/ticker";

        // 
        public BitmartSubscription(IFuturesSymbol[] aSymbols, string strChannel, bool bSubscribe) 
        {
            Action = (bSubscribe ? SUBSCRIBE : UNSUBSCRIBE);
            List<string> aArguments = new List<string>();
            foreach(IFuturesSymbol oSymbol in aSymbols)
            {
                aArguments.Add( $"{strChannel}:{oSymbol.Symbol}");
            }
            Arguments = aArguments.ToArray();   
        }
        [JsonProperty("action")]
        public string Action { get; }
        [JsonProperty("args")]
        public string[] Arguments { get; }
    }
}

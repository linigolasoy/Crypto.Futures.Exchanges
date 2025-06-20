using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Ws
{

    /*
{
    "op":"subscribe",
    "args":[
        {
            "channel":"trades",
            "instId":"ETH-USDT"
        }
    ]
}     */




    internal class BlofinSubscriptionChannel
    {
        [JsonProperty("channel")]
        public string Channel { get; set; } = string.Empty;
        [JsonProperty("instId")]
        public string Symbol { get; set; } = string.Empty;
    }


    internal class BlofinSubscription
    {
        private const string OPTION_SUBSCRIBE = "subscribe";
        private const string OPTION_UNSUBSCRIBE = "unsubscribe";

        public const string CHANNEL_TRADES  = "trades";
        public const string CHANNEL_TICKERS = "tickers";
        public const string CHANNEL_FUNDING = "funding-rate";

        // private const string CHANNEL_TRADE = "trades";
        public BlofinSubscription(bool bSubscibe, IFuturesSymbol oSymbol, BarTimeframe eFrame)
        {
            Option = (bSubscibe ? OPTION_SUBSCRIBE : OPTION_UNSUBSCRIBE);

            Channels.Add(new BlofinSubscriptionChannel() { Channel = CHANNEL_TICKERS, Symbol = oSymbol.Symbol });
            Channels.Add(new BlofinSubscriptionChannel() { Channel = CHANNEL_FUNDING, Symbol = oSymbol.Symbol });
        }
        [JsonProperty("op")]
        public string Option { get; }

        [JsonProperty("args")]
        public List<BlofinSubscriptionChannel> Channels { get; } = new List<BlofinSubscriptionChannel>();   
    }
}

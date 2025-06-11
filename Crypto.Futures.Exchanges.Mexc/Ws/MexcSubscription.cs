using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Ws
{

    internal class BarSubscriptionParam
    {
        public BarSubscriptionParam(IFuturesSymbol oSymbol, BarTimeframe eFrame )
        {
            Symbol = oSymbol.Symbol;
            var eInterval = MexcHistory.TimeframeToMexc(eFrame);
            if (eInterval == null) throw new ArgumentException("Invalid time interval");
            Interval = eInterval.Value.ToString();
        }
        [JsonProperty("symbol")]
        public string Symbol { get; }
        [JsonProperty("interval")]
        public string Interval { get; private set; } = string.Empty;
    }


    /// <summary>
    /// Method for subscription
    /// </summary>
    internal class MexcMethod
    {
        public MexcMethod( string strMethod, JObject? oParam )
        {
            Method = strMethod;
            Param = oParam;
        }
        [JsonProperty("method")]
        public string Method { get; }
        [JsonProperty("param", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JObject? Param { get; } = null;

    }
}

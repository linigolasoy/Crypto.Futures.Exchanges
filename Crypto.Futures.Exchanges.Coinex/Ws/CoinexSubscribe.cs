using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Ws
{

    internal class CoinexMessage
    {
        [JsonProperty("id")]
        public int? Id { get; set; } = null;
        [JsonProperty("code")]
        public int? Code { get; set; } = null;
        [JsonProperty("message")]
        public string? Message { get; set; } = null;

        [JsonProperty("method")]
        public string? Method { get; set; }

        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;
}

    internal class CoinexSubscribeJsonParams
    {
        public CoinexSubscribeJsonParams(string[] aSymbols) 
        { 
            Markets = aSymbols;
        }
        [JsonProperty("market_list")]
        public string[] Markets { get; }
    }

    internal class CoinexSubscribeJson
    {
        public const string METHOD_FUNDING  = "state";
        public const string METHOD_DEALS    = "deals";
        public const string METHOD_BBO      = "bbo"; 
        public CoinexSubscribeJson(int nId, IFuturesSymbol[] aSymbols, string strMethod)
        {
            Id = nId;
            Method = $"{strMethod}.subscribe";
            string[] aSymbolString = aSymbols.Select(p=> p.Symbol).Distinct().ToArray();
            var oParams = new CoinexSubscribeJsonParams(aSymbolString);
            Params = JObject.FromObject(oParams);   
        }

        public CoinexSubscribeJson(int  nId, string strMethod)
        {
            Id = nId;
            Method = strMethod;
            Params = new JObject(); 
        }
        [JsonProperty("method")]
        public string Method { get; }
        [JsonProperty("id")]
        public int Id { get; }
        [JsonProperty("params")]
        JObject Params { get; }
        /*
{
    "method": "state.subscribe",
    "params": {"market_list": ["BTCUSDT"]},
    "id": 1
}
         */
    }
}

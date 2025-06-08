using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Crypto.Futures.Exchanges.Coinex
{

    internal class CoinexFundingRateJson
    {
        [JsonProperty("market")]
        public string Market { get; set; } =string.Empty;
        [JsonProperty("mark_price")]
        public string MarkPrice { get; set; } = string.Empty;
        [JsonProperty("latest_funding_rate")]
        public string LatestFundingRate { get; set; } = string.Empty;
        [JsonProperty("next_funding_rate")]
        public string NextFundingRate { get; set; } = string.Empty;
        [JsonProperty("max_funding_rate")]
        public string MaxFundingRate { get; set; } = string.Empty;
        [JsonProperty("min_funding_rate")]
        public string MinFundingRate { get; set; } = string.Empty;
        [JsonProperty("latest_funding_time")]
        public long LatestFundingTime { get; set; } = 0;
        [JsonProperty("next_funding_time")]
        public long NextFundingTime { get; set; } = 0;
    }

    internal class CoinexFundingRate : IFundingRate
    {

        public CoinexFundingRate( IFuturesSymbol oSymbol, CoinexFundingRateJson oJson ) 
        { 
            Symbol = oSymbol;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.LatestFundingTime);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            Next = dDate;
            Rate = decimal.Parse(oJson.LatestFundingRate, CultureInfo.InvariantCulture);

        }
        public IFuturesSymbol Symbol { get; }

        public DateTime Next { get; }

        public decimal Rate { get; }

        public static IFundingRate? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            var oFundingJson = oToken.ToObject<CoinexFundingRateJson>();
            if (oFundingJson == null) return null;
            var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Market);
            if (oSymbol == null) return null;
            return new CoinexFundingRate(oSymbol, oFundingJson);
        }
    }
}

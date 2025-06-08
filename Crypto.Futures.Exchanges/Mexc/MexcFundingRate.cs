using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Crypto.Futures.Exchanges.Mexc
{
    internal class MexcFundingRateJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; } = 0m;
        [JsonProperty("maxFundingRate")]
        public decimal MaxFundingRate { get; set; } = 0m;
        [JsonProperty("minFundingRate")]
        public decimal MinFundingRate { get; set; } = 0m;
        [JsonProperty("collectCycle")]
        public int CollectCycle { get; set; } = 0;
        [JsonProperty("nextSettleTime")]
        public long NextSettleTime { get; set; } = 0L;
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; } = 0L;
    }

    /// <summary>
    /// Funding rate implementation for Mexc
    /// </summary>
    internal class MexcFundingRate : IFundingRate
    {
        public MexcFundingRate( IFuturesSymbol oSymbol, MexcFundingRateJson oJson ) 
        { 
            Symbol = oSymbol;
            Rate = oJson.FundingRate;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.NextSettleTime);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            Next = dDate;
        }

        public MexcFundingRate(IFuturesSymbol oSymbol, MexcTickerJson oJson)
        {
            Symbol = oSymbol;
            Rate = oJson.FundingRate;
            Next = Util.NextFundingRate(8);
        }

        public IFuturesSymbol Symbol { get; }

        public DateTime Next { get; }

        public decimal Rate { get; }


        public static IFundingRate? Parse(IFuturesExchange oExchange, JToken? oJson, bool bTicker)
        {
            if (oJson == null) return null;
            if (!bTicker)
            {
                var oFundingJson = oJson.ToObject<MexcFundingRateJson>();
                if (oFundingJson == null) return null;
                var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Symbol);
                if (oSymbol == null) return null;
                return new MexcFundingRate(oSymbol, oFundingJson);
            }
            else
            {
                var oTickerJson = oJson.ToObject<MexcTickerJson>();
                if (oTickerJson == null) return null;
                var oSymbol = oExchange.SymbolManager.GetSymbol(oTickerJson.Symbol);
                if (oSymbol == null) return null;
                return new MexcFundingRate(oSymbol, oTickerJson);
            }
        }   
    }
}

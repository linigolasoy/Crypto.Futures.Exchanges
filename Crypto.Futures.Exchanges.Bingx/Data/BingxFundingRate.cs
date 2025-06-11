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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bingx.Data
{


    internal class BingxFundingRateJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonProperty("lastFundingRate")] // Last updated funding rate
        public string LastFundingRate { get; set; } = string.Empty;
        [JsonProperty("markPrice")] // current mark price
        public string MarkPrice { get; set; } = string.Empty;
        [JsonProperty("indexPrice")] // index price
        public string IndexPrice { get; set; } = string.Empty;
        [JsonProperty("nextFundingTime")] // The remaining time for the nex...
        public long NextFundingTime { get; set; } = 0;

    }
    internal class BingxFundingRate : IFundingRate
    {

        public BingxFundingRate(IFuturesSymbol oSymbol, BingxFundingRateJson oJson )
        {
            Symbol = oSymbol;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.NextFundingTime);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            Next = dDate;
            Rate = decimal.Parse(oJson.LastFundingRate, CultureInfo.InvariantCulture);  
        }
        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get => WsMessageType.FundingRate; }
        public DateTime Next { get; }

        public decimal Rate { get; }

        public static IFundingRate? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            var oFundingJson = oToken.ToObject<BingxFundingRateJson>();
            if (oFundingJson == null) return null;
            var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Symbol);
            if (oSymbol == null) return null;
            return new BingxFundingRate(oSymbol, oFundingJson);
        }
    }
}

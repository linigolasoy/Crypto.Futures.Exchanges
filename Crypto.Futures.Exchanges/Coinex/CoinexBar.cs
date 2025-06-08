using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crypto.Futures.Exchanges.Coinex
{

    internal class CoinexBarJson
    {
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        [JsonProperty("created_at")]
        public long CreatedAt { get; set; } = 0;
        [JsonProperty("open")]
        public string Open { get; set; } = string.Empty;
        [JsonProperty("close")]
        public string Close { get; set; } = string.Empty;
        [JsonProperty("high")]
        public string High { get; set; } = string.Empty;
        [JsonProperty("low")]
        public string Low { get; set; } = string.Empty;
        [JsonProperty("volume")]
        public string Volume { get; set; } = string.Empty;
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }
    internal class CoinexBar : FuturesBar
    {

        public CoinexBar(
                IFuturesSymbol oSymbol,
                BarTimeframe eFrame
            ) : base(oSymbol, eFrame)
        {
        }

        public static IBar? Parse(IFuturesExchange oExchange, IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oJson)
        {
            if (oJson == null) return null;
            var oParsed = oJson.ToObject<CoinexBarJson>();
            if (oParsed == null) return null;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oParsed.CreatedAt);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            CoinexBar oBar = new CoinexBar(oSymbol, eFrame);

            oBar.DateTime = dDate;
            oBar.Open = decimal.Parse(oParsed.Open, CultureInfo.InvariantCulture);
            oBar.Close = decimal.Parse(oParsed.Close, CultureInfo.InvariantCulture);
            oBar.High = decimal.Parse(oParsed.High, CultureInfo.InvariantCulture);
            oBar.Low = decimal.Parse(oParsed.Low, CultureInfo.InvariantCulture);
            oBar.Volume = decimal.Parse(oParsed.Volume, CultureInfo.InvariantCulture);
            return oBar;
        }
    }
}

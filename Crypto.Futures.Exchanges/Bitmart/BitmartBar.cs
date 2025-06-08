using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Crypto.Futures.Exchanges.Bitmart
{

    internal class BitmartBarJson
    {
        [JsonProperty("timestamp")]
        public long TimeStamp { get; set; } = 0;
        [JsonProperty("open_price")]
        public string Open { get; set; } = string.Empty;
        [JsonProperty("close_price")]
        public string Close { get; set; } = string.Empty;
        [JsonProperty("high_price")]
        public string High { get; set; } = string.Empty;
        [JsonProperty("low_price")]
        public string Low { get; set; } = string.Empty;
        [JsonProperty("volume")]
        public string Volume { get; set; } = string.Empty;
    }
    internal class BitmartBar: FuturesBar
    {
        public BitmartBar(IFuturesSymbol oSymbol, BarTimeframe eFrame) : base(oSymbol, eFrame)
        {
        }

        public static IBar? Parse( IFuturesExchange oExchange, IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oToken )
        {
            if (oToken == null) return null;
            BitmartBarJson? oJson = oToken.ToObject<BitmartBarJson>();
            if (oJson == null) return null;
            BitmartBar oBar = new BitmartBar(oSymbol, eFrame);
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeSeconds(oJson.TimeStamp);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            oBar.DateTime = dDate;
            oBar.Open = decimal.Parse(oJson.Open, CultureInfo.InvariantCulture);
            oBar.Close = decimal.Parse(oJson.Close, CultureInfo.InvariantCulture);
            oBar.High = decimal.Parse(oJson.High, CultureInfo.InvariantCulture);
            oBar.Low = decimal.Parse(oJson.Low, CultureInfo.InvariantCulture);
            oBar.Volume = decimal.Parse(oJson.Volume, CultureInfo.InvariantCulture);

            return oBar;
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bingx.Data
{
    internal class BingxLeverage
    {
        [JsonProperty("longLeverage")]
        public long LongLeverage { get; set; } = 0;
        [JsonProperty("shortLeverage")]
        public long ShortLeverage { get; set; } = 0;
        [JsonProperty("maxLongLeverage")]
        public long MaxLongLeverage { get; set; } = 0;
        [JsonProperty("maxShortLeverage")]
        public long MaxShortLeverage { get; set; } = 0;
        [JsonProperty("availableLongVol")]
        public string AvailableLongVol { get; set; } = string.Empty;
        [JsonProperty("availableShortVol")]
        public string AvailableShortVol { get; set; } = string.Empty;
        [JsonProperty("availableLongVal")]
        public string AvailableLongVal { get; set; } = string.Empty;
        [JsonProperty("availableShortVal")]
        public string AvailableShortVal { get; set; } = string.Empty;
        [JsonProperty("maxPositionLongVal")]
        public string MaxPositionLongVal { get; set; } = string.Empty;
        [JsonProperty("maxPositionShortVal")]
        public string MaxPositionShortVal { get; set; } = string.Empty;

    }

    internal class BingxLeveragePost
    {

        [JsonProperty("leverage")]
        public long Leverage { get; set; } = 0;
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;

        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;

    }


}

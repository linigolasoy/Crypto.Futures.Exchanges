using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{
    internal class MexcLeverage
    {
        [JsonProperty("level")]
        public int Level { get; set; }
        [JsonProperty("maxVol")]
        public decimal MaxVol { get; set; }
        [JsonProperty("mmr")]
        public decimal Mmr { get; set; }
        [JsonProperty("imr")]
        public decimal Imr { get; set; }
        [JsonProperty("positionType")]
        public int PositionType { get; set; }
        [JsonProperty("openType")]
        public int OpenType { get; set; }
        [JsonProperty("leverage")]
        public decimal Leverage { get; set; }
        [JsonProperty("limitBySys")]
        public bool LimitBySys { get; set; }
        [JsonProperty("currentMmr")]
        public decimal CurrentMmr { get; set; }
    }

    internal class MexcLeveragePost
    {
        [JsonProperty("leverage")]
        public int Leverage { get; set; }
        [JsonProperty("openType")]
        public int OpenType { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;  
        [JsonProperty("positionType")]
        public int PositionType { get; set; }
    }
}

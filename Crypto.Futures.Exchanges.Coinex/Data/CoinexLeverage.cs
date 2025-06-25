using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Crypto.Futures.Exchanges.Coinex.Data
{
    internal class CoinexLeveragePost
    {
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        [JsonProperty("market_type")]
        public string MarketType { get; set; } = string.Empty;
        [JsonProperty("margin_mode")]
        public string MarginMode { get; set; } = string.Empty;
        [JsonProperty("leverage")]
        public int Leverage { get; set; } = 0;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Crypto.Futures.Exchanges.Mexc.Data
{
    internal class MexcTickerJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("lastPrice")]
        public decimal LastPrice { get; set; } = 0;
        [JsonProperty("bid1")]
        public decimal Bid1 { get; set; } = 0;
        [JsonProperty("ask1")]
        public decimal Ask1 { get; set; } = 0;
        [JsonProperty("volume24")]
        public decimal Volume24 { get; set; } = 0;
        [JsonProperty("amount24")]
        public decimal Amount24 { get; set; } = 0;
        [JsonProperty("holdVol")]
        public decimal HoldVol { get; set; } = 0;
        [JsonProperty("lower24Price")]
        public decimal Lower24Price { get; set; } = 0;
        [JsonProperty("high24Price")]
        public decimal High24Price { get; set; } = 0;
        [JsonProperty("riseFallRate")]
        public decimal RiseFallRate { get; set; } = 0;
        [JsonProperty("riseFallValue")]
        public decimal RiseFallValue { get; set; } = 0;
        [JsonProperty("indexPrice")]
        public decimal IndexPrice { get; set; } = 0;
        [JsonProperty("fairPrice")]
        public decimal FairPrice { get; set; } = 0;
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; } = 0;
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; } = 0;
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{
    internal class MexcMessage
    {
        [JsonProperty("channel")]   
        public string Channel { get; set; } = string.Empty;
        [JsonProperty("ts")]
        public string Timestamp { get; set; } = string.Empty;
        [JsonProperty("symbol")]
        public string? Symbol { get; set; } = null;
        [JsonProperty("data")]
        public JToken? Data { get; set; }= null;
    }
}

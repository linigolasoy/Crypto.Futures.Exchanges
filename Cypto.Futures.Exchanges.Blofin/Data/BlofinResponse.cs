using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Data
{
    internal class BlofinResponse
    {
        [JsonProperty("code")]
        public int code { get; set; }
        [JsonProperty("message")]
        public string? message { get; set; }
        [JsonProperty("data")]
        public JToken? data { get; set; }
    }
}

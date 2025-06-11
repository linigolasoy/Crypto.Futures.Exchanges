using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Ws
{
    internal class BlofinMessage
    {
        [JsonProperty("event")]
        public string? Event { get; set; } = null;
        [JsonProperty("arg")]
        public BlofinSubscriptionChannel? Argument { get; set; } = null;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;
    }
}

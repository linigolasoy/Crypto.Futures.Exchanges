using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{
    internal class MexcResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = false;
        [JsonProperty("code")]  
        public int code { get; set; } = 0;
        [JsonProperty("msg")]
        public string? msg { get; set; } = string.Empty;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null!;



        public static MexcResponse? FromJson(string strJson) =>
            JsonConvert.DeserializeObject<MexcResponse>(strJson);
    }
}

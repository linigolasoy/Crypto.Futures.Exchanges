using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitunix.Data
{
    internal class BitunixResponse
    {
        [JsonProperty("code")]
        public string code { get; set; } = string.Empty;
        [JsonProperty("msg")]
        public string? msg { get; set; } = string.Empty;
        [JsonProperty("data")]
        public JToken? data { get; set; } = null!;


        public bool IsSuccess()
        {
            return code.Equals("0") && msg != null && msg.Equals("Success");
        }

        public static BitunixResponse? FromJson(string strJson) =>
            JsonConvert.DeserializeObject<BitunixResponse>(strJson);
    }
}

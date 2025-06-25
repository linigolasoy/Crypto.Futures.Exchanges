using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    internal class BitgetLeveragePost
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("productType")]
        public string ProductType { get; set; } = "USDT-FUTURES";
        [JsonProperty("marginCoin")]
        public string MarginCoin { get; set; } = string.Empty;
        [JsonProperty("leverage")]
        public string Leverage { get; set; } = string.Empty;
    }
}

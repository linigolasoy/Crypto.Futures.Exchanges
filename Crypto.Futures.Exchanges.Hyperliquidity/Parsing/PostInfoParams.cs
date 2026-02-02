using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Parsing
{
    internal class PostInfoParams
    {
        [JsonProperty("type")]
        public string InfoType { get; set; } = "metaAndAssetCtxs";
    }
}

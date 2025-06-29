using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{

    internal enum MexcOrderSide
    {
        OpenLong = 1,   // open long
        CloseShort = 2, // close short
        OpenShort = 3,  // open short
        CloseLong = 4   // close long   
    }

    internal enum MexcOrderType
    {
        Limit = 1,
        Market = 5
    }

    internal enum MexcOpenType
    {
        Isolated = 1, // isolated
        Cross = 2     // cross
    }
    internal class MexcOrderJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("price")]
        public decimal Price { get; set; } = 0;
        [JsonProperty("vol")]
        public decimal Vol { get; set; }
        [JsonProperty("leverage", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? Leverage { get; set; }
        [JsonProperty("side")]
        public int Side { get; set; } // 1 open long ,2 close short,3 open short ,4 close long
        [JsonProperty("type")]
        public int Type { get; set; } // 1:price limited order,2:Post Only Maker,3:transact or cancel instantly,4 : transact completely or cancel completely，5:market orders,6 convert market price to current price
        [JsonProperty("openType")]
        public int OpenType { get; set; } // 1:isolated,2:cross
        [JsonProperty("positionId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long? PositionId { get; set; } // position Id，It is recommended to fill in this parameter when closing a position
        [JsonProperty("externalOid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? ExternalOid { get; set; } // external order ID
        [JsonProperty("stopLossPrice", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal? StopLossPrice { get; set; } // stop-loss price
        [JsonProperty("takeProfitPrice", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public decimal? TakeProfitPrice { get; set; } // take-profit price
        [JsonProperty("positionMode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? PositionMode { get; set; } // position mode, 1:hedge, 2:one-way, default: the user's current config
        [JsonProperty("reduceOnly", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? ReduceOnly { get; set; } // Default false, For one-way positions, if you need to only reduce positions, pass in true, and two-way positions will not accept this parameter.

    }
}

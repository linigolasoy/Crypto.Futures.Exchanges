using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Data
{

    internal class BingxBalanceJson
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;
        [JsonProperty("asset")]
        public string Asset { get; set; } = string.Empty;
        [JsonProperty("balance")]
        public string Balance { get; set; } = string.Empty;
        [JsonProperty("equity")]
        public string Equity { get; set; } = string.Empty;
        [JsonProperty("unrealizedProfit")]
        public string UnrealizedProfit { get; set; } = string.Empty;
        [JsonProperty("realisedProfit")]
        public string RealisedProfit { get; set; } = string.Empty;
        [JsonProperty("availableMargin")]
        public string AvailableMargin { get; set; } = string.Empty;
        [JsonProperty("usedMargin")]
        public string UsedMargin { get; set; } = string.Empty;
        [JsonProperty("freezedMargin")]
        public string FreezedMargin { get; set; } = string.Empty;
        [JsonProperty("shortUid")]
        public string ShortUid { get; set; } = string.Empty;
    }
    internal class BingxBalance : IBalance
    {
        internal BingxBalance( IFuturesExchange oExchange, BingxBalanceJson oJson) 
        { 
            Exchange = oExchange;
            Currency = oJson.Asset;
            Avaliable = decimal.Parse(oJson.AvailableMargin, CultureInfo.InvariantCulture);
            decimal nLocked = decimal.Parse(oJson.UsedMargin, CultureInfo.InvariantCulture);
            nLocked += decimal.Parse(oJson.FreezedMargin, CultureInfo.InvariantCulture);
            Locked = nLocked;
            Balance = decimal.Parse(oJson.Equity, CultureInfo.InvariantCulture);
        }

        public IFuturesExchange Exchange { get; }

        public string Currency { get; }

        public decimal Balance { get; }

        public decimal Locked { get; }

        public decimal Avaliable { get; }

        public static IBalance? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if( oToken == null ) return null;
            BingxBalanceJson? oJson = oToken.ToObject<BingxBalanceJson>();
            if( oJson == null ) return null;
            IBalance oBalance = new BingxBalance(oExchange, oJson);
            if( oBalance.Balance <= 0 && oBalance.Locked <= 0 && oBalance.Avaliable <= 0) return null;
            return oBalance;
        }
    }
}

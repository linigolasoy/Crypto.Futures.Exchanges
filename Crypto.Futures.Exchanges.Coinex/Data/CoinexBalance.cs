using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Data
{

    internal class CoinexBalanceJson
    {
        [JsonProperty("available")]
        public string Avaliable {  get; set; } = string.Empty;
        [JsonProperty("ccy")]
        public string Currency { get; set; } = string.Empty;
        [JsonProperty("frozen")]
        public string Frozen { get; set; } = string.Empty;
        [JsonProperty("margin")]
        public string Margin { get; set; } = string.Empty;
        [JsonProperty("transferrable")]
        public string Transferrable { get; set; } = string.Empty;
        [JsonProperty("unrealized_pnl")]
        public string UnrealizedPnl { get; set; } = string.Empty;
    }
    internal class CoinexBalance : IBalance
    {
        internal CoinexBalance( IFuturesExchange oExchange, CoinexBalanceJson oJson )
        {
            Exchange = oExchange;
            Currency = oJson.Currency;
            Avaliable = decimal.Parse(oJson.Avaliable, CultureInfo.InvariantCulture);
            Locked = decimal.Parse(oJson.Frozen, CultureInfo.InvariantCulture) +
                      decimal.Parse(oJson.Margin, CultureInfo.InvariantCulture);
            Balance = Avaliable + Locked;
        }
        public IFuturesExchange Exchange { get; }

        public string Currency { get; }

        public decimal Balance { get; }

        public decimal Locked { get; }

        public decimal Avaliable { get; }

        public static IBalance? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            CoinexBalanceJson? oJson = oToken.ToObject<CoinexBalanceJson>();
            if (oJson == null) return null;
            IBalance oBalance = new CoinexBalance(oExchange, oJson);
            if( oBalance.Balance + oBalance.Locked + oBalance.Avaliable <= 0 ) return null;
            return oBalance;
        }
    }
}

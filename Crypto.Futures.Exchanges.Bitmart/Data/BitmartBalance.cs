using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartBalanceJson
    {
        [JsonProperty("currency")]
        public string Currency {  get; set; } = string.Empty;
        [JsonProperty("position_deposit")]
        public string Position_deposit { get; set; } = string.Empty;
        [JsonProperty("frozen_balance")]
        public string Frozen_balance { get; set; } = string.Empty;
        [JsonProperty("available_balance")]
        public string Available_balance { get; set; } = string.Empty;
        [JsonProperty("equity")]
        public string Equity { get; set; } = string.Empty;
        [JsonProperty("unrealized")]
        public string Unrealized { get; set; } = string.Empty;
    }
    internal class BitmartBalance : IBalance
    {
        internal BitmartBalance( IFuturesExchange oExchange, BitmartBalanceJson oJson) 
        { 
            Exchange = oExchange;
            Currency = oJson.Currency;
            Balance = decimal.Parse(oJson.Equity, CultureInfo.InvariantCulture);
            Avaliable = decimal.Parse(oJson.Available_balance, CultureInfo.InvariantCulture);
            Locked = Balance - Avaliable;
        }
        public IFuturesExchange Exchange { get; }

        public string Currency { get; }

        public decimal Balance { get; }

        public decimal Locked { get; }

        public decimal Avaliable { get; }

        public static IBalance? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if ( oToken == null ) return null;
            BitmartBalanceJson? oJson = oToken.ToObject<BitmartBalanceJson>();
            if ( oJson == null ) return null;
            IBalance oResult = new BitmartBalance(oExchange, oJson);
            if( oResult.Balance + oResult.Locked + oResult.Avaliable <= 0 ) return null;
            return oResult;
        }
    }
}

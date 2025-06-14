using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{

    internal class BitgetBalanceJson
    {
        [JsonProperty("marginCoin")]
        public string Currency { get; set; } = string.Empty;    
        [JsonProperty("locked")]
        public string Locked { get; set; } = string.Empty;    

        [JsonProperty("available")] // >available String  Available quantity in the account
        public string Available { get; set; } = string.Empty;    

        [JsonProperty("crossedMaxAvailable")] // >crossedMaxAvailable String  Maximum available balance to open positions under the cross margin mode(margin coin)
        public string CrossedMaxAvailable { get; set; } = string.Empty;    

        [JsonProperty("isolatedMaxAvailable")] // >isolatedMaxAvailable String  Maximum available balance to open positions under the isolated margin mode(margin coin)
        public string IsolatedMaxAvailable { get; set; } = string.Empty;    

        [JsonProperty("maxTransferOut")] // >maxTransferOut String  Maximum transferable amount
        public string MaxTransferOut { get; set; } = string.Empty;    

        [JsonProperty("accountEquity")] // >accountEquity String  Account equity(margin coin), Includes unrealized PnL(based on mark price)
        public string AccountEquity { get; set; } = string.Empty;
        
        [JsonProperty("usdtEquity")] // >usdtEquity String  Account equity in USDT
        public string UsdtEquity { get; set; } = string.Empty;

        [JsonProperty("btcEquity")] // >btcEquity String  Account equity in BTC
        public string BtcEquity { get; set; } = string.Empty;
        [JsonProperty("crossedRiskRate")]
        public string CrossedRiskRate { get; set; } = string.Empty;
        [JsonProperty("unrealizedPL")]
        public string UnrealizedPL { get; set; } = string.Empty;
        [JsonProperty("coupon")]
        public string Coupon { get; set; } = string.Empty;
        [JsonProperty("unionTotalMargin")]
        public string UnionTotalMargin { get; set; } = string.Empty;
        [JsonProperty("unionAvailable")]
        public string UnionAvailable { get; set; } = string.Empty;
        [JsonProperty("unionMm")]
        public string UnionMm { get; set; } = string.Empty;
        [JsonProperty("isolatedMargin")]
        public string IsolatedMargin { get; set; } = string.Empty;
        [JsonProperty("crossedMargin")]
        public string CrossedMargin { get; set; } = string.Empty;
        [JsonProperty("crossedUnrealizedPL")]
        public string CrossedUnrealizedPL { get; set; } = string.Empty;
        [JsonProperty("isolatedUnrealizedPL")]
        public string IsolatedUnrealizedPL { get; set; } = string.Empty;
        [JsonProperty("assetMode")]
        public string AssetMode { get; set; } = string.Empty;
    }
    internal class BitgetBalance : IBalance
    {
        internal BitgetBalance( IFuturesExchange oExchange, BitgetBalanceJson oJson )
        {
            Exchange = oExchange;
            Currency = oJson.Currency;
            Balance = decimal.Parse(oJson.AccountEquity, CultureInfo.InvariantCulture);
            Locked = decimal.Parse(oJson.Locked, CultureInfo.InvariantCulture);
            Avaliable = decimal.Parse(oJson.Available, CultureInfo.InvariantCulture);
        }
        public IFuturesExchange Exchange { get; }

        public string Currency { get; }

        public decimal Balance { get; }

        public decimal Locked { get; }

        public decimal Avaliable { get; }

        public static IBalance? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if( oToken == null ) return null;
            BitgetBalanceJson? oJson = oToken.ToObject<BitgetBalanceJson>();
            if( oJson == null ) return null;
            IBalance oResult = new BitgetBalance(oExchange, oJson);
            if( oResult.Balance + oResult.Locked + oResult.Avaliable <= 0 ) return null;
            return oResult;
        }
    }
}

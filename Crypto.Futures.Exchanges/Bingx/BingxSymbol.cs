using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

namespace Crypto.Futures.Exchanges.Bingx
{

    internal class BingxSymbolJson
    {
        [JsonProperty("contractId")] // contract ID
        public string ContractId { get; set; } = string.Empty;
        [JsonProperty("symbol")] // trading pair, for example: BTCUSDT
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("quantityPrecision")] // transaction quantity precision
        public int QuantityPrecision { get; set; } = 0;
        [JsonProperty("pricePrecision")] // price precision
        public int PricePrecision { get; set; } = 0;
        [JsonProperty("takerFeeRate")] // take transaction fee
        public double TakerFeeRate { get; set; } = 0.0f;
        [JsonProperty("makerFeeRate")] // make transaction fee
        public double MakerFeeRate { get; set; } = 0.0f;
        [JsonProperty("tradeMinQuantity")] // minimum trading unit(COIN)
        public double TradeMinQuantity { get; set; } = 0.0f;
        [JsonProperty("tradeMinUSDT")] // minimum trading unit(USDT)
        public double TradeMinUSDT { get; set; } = 0.0f;
        [JsonProperty("currency")] // settlement and margin currency...
        public string Currency { get; set; } = string.Empty;
        [JsonProperty("asset")] // contract trading asset
        public string Asset { get; set; } = string.Empty;
        [JsonProperty("status")] // 1 online, 25 forbidden to open...
        public int Status { get; set; } = 0;
        [JsonProperty("apiStateOpen")] // Whether the API can open a position
        public string ApiStateOpen { get; set; } = string.Empty;
        [JsonProperty("apiStateClose")] // Whether API can close position
        public string ApiStateClose { get; set; } = string.Empty;
        [JsonProperty("ensureTrigger")] // Whether to support guaranteed stop loss
        public bool EnsureTrigger { get; set; } = false;
        [JsonProperty("triggerFeeRate")] // The fee rate for guaranteed stop loss
        public string TriggerFeeRate { get; set; } = string.Empty;
        [JsonProperty("brokerState")] // Whether to prohibit broker use
        public bool BrokerState { get; set; } = false;
        [JsonProperty("launchTime")] // shelf time; The status of the contract is online
        public long LaunchTime { get; set; } = 0;
        [JsonProperty("maintainTime")] // The start time of the prohibited opening period
        public long MaintainTime { get; set; } = 0;
        [JsonProperty("offTime")] // Down line time, after the time is down line
        public long OffTime { get; set; } = 0;
    }
    internal class BingxSymbol: BaseSymbol
    {
        public BingxSymbol( IFuturesExchange oExchange, BingxSymbolJson oJson ):
            base(oExchange, oJson.Symbol, oJson.Asset, oJson.Currency)
        {
            LeverageMin = 1;
            LeverageMax = 100;
            FeeMaker = (decimal)oJson.MakerFeeRate;
            FeeTaker = (decimal)oJson.TakerFeeRate;
            Decimals = oJson.PricePrecision;
            ContractSize = 1; // Bingx does not use contract size
            UseContractSize = false; // Bingx does not use contract size
            QuantityDecimals = oJson.QuantityPrecision;
            Minimum = (decimal)oJson.TradeMinQuantity; // Minimum trading unit in USDT
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.LaunchTime);
            DateTime dDate = oOffset.Date.ToLocalTime();
            ListDate = dDate;
        }

        public static IFuturesSymbol? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            if (!(oToken is JObject)) return null;
            BingxSymbolJson? oJson = oToken.ToObject<BingxSymbolJson>();
            if (oJson == null) return null;

            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.LaunchTime);
            DateTime dDate = oOffset.Date.ToLocalTime();
            if (dDate > DateTime.Now) return null;
            return new BingxSymbol(oExchange, oJson);
        }

    }
}

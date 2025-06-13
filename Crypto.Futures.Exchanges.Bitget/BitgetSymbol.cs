using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Crypto.Futures.Exchanges.Bitget
{

    internal class BitgetSymbolJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("baseCoin")]
        public string BaseCoin { get; set; } = string.Empty;
        [JsonProperty("quoteCoin")]
        public string QuoteCoin { get; set; } = string.Empty;
        [JsonProperty("buyLimitPriceRatio")]
        public string BuyLimitPriceRatio { get; set; } = string.Empty;
        [JsonProperty("sellLimitPriceRatio")]
        public string SellLimitPriceRatio { get; set; } = string.Empty;
        [JsonProperty("feeRateUpRatio")]
        public string FeeRateUpRatio { get; set; } = string.Empty;
        [JsonProperty("makerFeeRate")]
        public string MakerFeeRate { get; set; } = string.Empty;
        [JsonProperty("takerFeeRate")]
        public string TakerFeeRate { get; set; } = string.Empty;
        [JsonProperty("openCostUpRatio")]
        public string OpenCostUpRatio { get; set; } = string.Empty;
        [JsonProperty("supportMarginCoins")]
        public List<string> SupportMarginCoins { get; set; } = new List<string>();
        [JsonProperty("minTradeNum")]
        public string MinTradeNum { get; set; } = string.Empty;
        [JsonProperty("priceEndStep")]
        public string PriceEndStep { get; set; } = string.Empty;
        [JsonProperty("volumePlace")]
        public string VolumePlace { get; set; } = string.Empty;
        [JsonProperty("pricePlace")]
        public string PricePlace { get; set; } = string.Empty;
        [JsonProperty("sizeMultiplier")]
        public string SizeMultiplier { get; set; } = string.Empty;
        [JsonProperty("symbolType")]
        public string SymbolType { get; set; } = string.Empty;
        [JsonProperty("minTradeUSDT")]
        public string MinTradeUSDT { get; set; } = string.Empty;
        [JsonProperty("maxSymbolOrderNum")]
        public string MaxSymbolOrderNum { get; set; } = string.Empty;
        [JsonProperty("maxProductOrderNum")]
        public string MaxProductOrderNum { get; set; } = string.Empty;
        [JsonProperty("maxPositionNum")]
        public string MaxPositionNum { get; set; } = string.Empty;
        [JsonProperty("symbolStatus")]
        public string SymbolStatus { get; set; } = string.Empty; // listed, normal, maintain, limit_open, restrictedAPI, off
        [JsonProperty("offTime")]
        public string OffTime { get; set; } = string.Empty; // Removal time, '-1' means normal
        [JsonProperty("limitOpenTime")]
        public string LimitOpenTime { get; set; } = string.Empty; // Time to open positions, '-1' means normal; other values indicate that the symbol is under maintenance or to be maintained and trading is prohibited after the specified time.
        [JsonProperty("deliveryTime")]
        public string DeliveryTime { get; set; } = string.Empty; // Delivery time
        [JsonProperty("deliveryStartTime")]
        public string DeliveryStartTime { get; set; } = string.Empty; // Delivery start time
        [JsonProperty("deliveryPeriod")]
        public string DeliveryPeriod { get; set; } = string.Empty; // Delivery period, this_quarter current quarter, next_quarter second quarter
        [JsonProperty("launchTime")]
        public string LaunchTime { get; set; } = string.Empty; // Listing time
        [JsonProperty("fundInterval")]
        public string FundInterval { get; set; } = string.Empty; // Funding fee settlement cycle, hourly/every 8 hours
        [JsonProperty("minLever")]
        public string MinLever { get; set; } = string.Empty; // minimum leverage
        [JsonProperty("maxLever")]
        public string MaxLever { get; set; } = string.Empty; // Maximum leverage
        [JsonProperty("posLimit")]
        public string PosLimit { get; set; } = string.Empty; // Position limits
        [JsonProperty("maintainTime")]
        public string MaintainTime { get; set; } = string.Empty; // Maintenance time(there will be a value when the status is under maintenance/upcoming maintenance)

    }

    internal class BitgetSymbol : BaseSymbol
    {

        public BitgetSymbol(IFuturesExchange oExchange, BitgetSymbolJson oJson):
            base(oExchange, oJson.Symbol, oJson.BaseCoin, oJson.QuoteCoin)
        {

            this.LeverageMax = int.Parse(oJson.MaxLever);
            this.LeverageMin = int.Parse(oJson.MinLever);
            this.FeeMaker = decimal.Parse(oJson.MakerFeeRate, CultureInfo.InvariantCulture);
            this.FeeTaker = decimal.Parse(oJson.TakerFeeRate, CultureInfo.InvariantCulture);
            this.Decimals = int.Parse(oJson.PricePlace);
            this.ContractSize = 1; // decimal.Parse(oJson.SizeMultiplier, CultureInfo.InvariantCulture);
            this.UseContractSize = false; // Bitget uses contract size
            this.QuantityDecimals = int.Parse(oJson.VolumePlace);
            this.Minimum = decimal.Parse(oJson.MinTradeNum, CultureInfo.InvariantCulture);
            long nTime = (string.IsNullOrEmpty(oJson.LaunchTime)? 0: long.Parse(oJson.LaunchTime));
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(nTime);
            DateTime dDate = oOffset.Date.ToLocalTime();
            ListDate = dDate;
        }
        public static IFuturesSymbol? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            if (!(oToken is JObject)) return null;
            BitgetSymbolJson? oJson = oToken.ToObject<BitgetSymbolJson>();
            if (oJson == null) return null;
            if(!string.IsNullOrEmpty(oJson.LaunchTime))
            {
                DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(oJson.LaunchTime));
                DateTime dDate = oOffset.Date.ToLocalTime();
                if (dDate > DateTime.Now.AddDays(1) ) return null;

            }
            return new BitgetSymbol(oExchange, oJson);
        }
    }
}

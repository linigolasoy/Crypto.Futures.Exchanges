using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bitmart
{


    internal class BitmartSymbolJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("product_type")] // product_type Int Contract type -1=perpetual -2=futures
        public int ProductType { get; set; } = 0;
        [JsonProperty("base_currency")]
        public string BaseCurrency { get; set; } = string.Empty;
        [JsonProperty("quote_currency")]
        public string QuoteCurrency { get; set; } = string.Empty;
        [JsonProperty("volume_precision")]
        public string VolumePrecision { get; set; } = string.Empty;
        [JsonProperty("price_precision")]
        public string PricePrecision { get; set; } = string.Empty;
        [JsonProperty("max_volume")]
        public string MaxVolume { get; set; } = string.Empty;
        [JsonProperty("market_max_volume")]
        public string MarketMaxVolume { get; set; } = string.Empty;
        [JsonProperty("min_volume")]
        public string MinVolume { get; set; } = string.Empty;
        [JsonProperty("contract_size")]
        public string ContractSize { get; set; } = string.Empty;
        [JsonProperty("index_price")]
        public string IndexPrice { get; set; } = string.Empty;
        [JsonProperty("index_name")]
        public string IndexName { get; set; } = string.Empty;
        [JsonProperty("min_leverage")]
        public string MinLeverage { get; set; } = string.Empty;
        [JsonProperty("max_leverage")]
        public string MaxLeverage { get; set; } = string.Empty;
        [JsonProperty("turnover_24h")]
        public string Turnover24h { get; set; } = string.Empty;
        [JsonProperty("volume_24h")]
        public string Volume24h { get; set; } = string.Empty;
        [JsonProperty("last_price")]
        public string LastPrice { get; set; } = string.Empty;
        [JsonProperty("open_timestamp")]
        public long OpenTimestamp { get; set; } = 0;
        [JsonProperty("expire_timestamp")]
        public long? ExpireTimestamp { get; set; } = null; // If null is returned, it does not expire
        [JsonProperty("settle_timestamp")]
        public long? SettleTimestamp { get; set; } = null; // If null is returned, it will not be automatically settlement
        [JsonProperty("funding_rate")]
        public string FundingRate { get; set; } = string.Empty;
        [JsonProperty("expected_funding_rate")]
        public string ExpectedFundingRate { get; set; } = string.Empty;
        [JsonProperty("open_interest")]
        public string OpenInterest { get; set; } = string.Empty;
        [JsonProperty("open_interest_value")]
        public string OpenInterestValue { get; set; } = string.Empty;
        [JsonProperty("high_24h")]
        public string High24h { get; set; } = string.Empty;
        [JsonProperty("low_24h")]
        public string Low24h { get; set; } = string.Empty;
        [JsonProperty("change_24h")]
        public string Change24h { get; set; } = string.Empty;
        [JsonProperty("funding_interval_hours")]
        public int FundingIntervalHours { get; set; } = 0;
    }

    internal class BitmartSymbol : BaseSymbol
    {
        public BitmartSymbol(IFuturesExchange oExchange, BitmartSymbolJson json):
            base(oExchange, json.Symbol, json.BaseCurrency, json.QuoteCurrency)
        {
            // Leverage
            LeverageMax = int.Parse(json.MaxLeverage);
            LeverageMin = int.Parse(json.MinLeverage);
            // Fees
            FeeMaker = 0.0002m; // Bitmart does not provide maker fee in the symbol info
            FeeTaker = 0.0006m; // Bitmart does not provide taker fee in the symbol info
            // Precision
            decimal nPricePrecision = (string.IsNullOrEmpty(json.PricePrecision)? 1: decimal.Parse(json.PricePrecision, CultureInfo.InvariantCulture));
            decimal nVolumePrecision = (string.IsNullOrEmpty(json.VolumePrecision) ? 1 : decimal.Parse(json.VolumePrecision, CultureInfo.InvariantCulture));

            Decimals = (int) Math.Log10((double)nPricePrecision) * -1;
            QuantityDecimals = (int)Math.Log10((double)nVolumePrecision) * -1;
            // Contract size
            ContractSize = decimal.Parse(json.ContractSize, CultureInfo.InvariantCulture);
            UseContractSize = true;
            // Minimum order size
            Minimum = decimal.Parse(json.MinVolume, CultureInfo.InvariantCulture);

            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(json.OpenTimestamp);
            DateTime dDate = oOffset.Date.ToLocalTime();
            ListDate = dDate;

        }

        public static IFuturesSymbol? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            if (!(oToken is JObject)) return null;

            BitmartSymbolJson? oJson = oToken.ToObject<BitmartSymbolJson>();
            if (oJson == null) return null;
            if( oJson.OpenTimestamp > 0 )
            {
                DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.OpenTimestamp);
                DateTime dDate = oOffset.Date.ToLocalTime();
                if (dDate > DateTime.Now.AddDays(1)) return null;

            }

            return new BitmartSymbol(oExchange, oJson);
        }
    }
}

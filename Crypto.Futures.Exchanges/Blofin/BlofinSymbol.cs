using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin
{

    internal class BlofinSymbolJson
    {
        [JsonProperty("instId")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("baseCurrency")]
        public string BaseCurrency { get; set; } = string.Empty;
        [JsonProperty("quoteCurrency")]
        public string QuoteCurrency { get; set; } = string.Empty;
        [JsonProperty("contractValue")]
        public string ContractValue { get; set; } = string.Empty;
        [JsonProperty("listTime")]
        public string ListTime { get; set; } = string.Empty;
        [JsonProperty("expireTime")]
        public string ExpireTime { get; set; } = string.Empty;
        [JsonProperty("maxLeverage")]
        public string MaxLeverage { get; set; } = string.Empty;
        [JsonProperty("minSize")] // Minimum order size in contracts(e.g. 0.1 contracts = 0.0001 BTC for BTC-USDT)
        public string MinSize { get; set; } = string.Empty;
        [JsonProperty("lotSize")]
        public string LotSize { get; set; } = string.Empty;
        [JsonProperty("tickSize")]
        public string TickSize { get; set; } = string.Empty;
        [JsonProperty("instType")]
        public string InstType { get; set; } = string.Empty;
        [JsonProperty("contractType")] // Contract type linear: linear contract inverse: inverse contract
        public string ContractType { get; set; } = string.Empty;
        [JsonProperty("maxLimitSize")]
        public string MaxLimitSize { get; set; } = string.Empty;
        [JsonProperty("maxMarketSize")]
        public string MaxMarketSize { get; set; } = string.Empty;
        [JsonProperty("state")]
        public string State { get; set; } = string.Empty; // Instrument status live suspend
    }
    internal class BlofinSymbol : BaseSymbol
    {
        public BlofinSymbol(IFuturesExchange oExchange, BlofinSymbolJson oJson):
            base(oExchange, oJson.Symbol, oJson.BaseCurrency, oJson.QuoteCurrency)
        {

            ContractSize = decimal.Parse(oJson.ContractValue, CultureInfo.InvariantCulture);
            LeverageMin = 1;
            LeverageMax = int.Parse(oJson.MaxLeverage);
            FeeMaker = 0.0002M;
            FeeTaker = 0.0006M;

            decimal nPricePrecision = (string.IsNullOrEmpty(oJson.TickSize) ? 1 : decimal.Parse(oJson.TickSize, CultureInfo.InvariantCulture));
            decimal nVolumePrecision = (string.IsNullOrEmpty(oJson.LotSize) ? 1 : decimal.Parse(oJson.LotSize, CultureInfo.InvariantCulture));

            Decimals = (int)Math.Log10((double)nPricePrecision) * -1;
            QuantityDecimals = (int)Math.Log10((double)nVolumePrecision) * -1;
            UseContractSize = true;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(oJson.ListTime));
            DateTime dDate = oOffset.Date.ToLocalTime();
            ListDate = dDate;

            // Decimals = oJson.PriceScale;
            // QuantityDecimals = oJson.VolScale;
        }



        public static IFuturesSymbol? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            if (!(oToken is JObject)) return null;
            BlofinSymbolJson? oJson = oToken.ToObject<BlofinSymbolJson>();
            if (oJson == null) return null;

            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(oJson.ListTime));
            DateTime dDate = oOffset.Date.ToLocalTime();
            if (dDate > DateTime.Now.AddDays(1)) return null;
            if( dDate < DateTime.Today && oJson.State == "suspend")
            {
                return null;
            }
            IFuturesSymbol oSymbol = new BlofinSymbol(oExchange, oJson);

            if( oSymbol.LeverageMax <= 5) return null;  
            return oSymbol;
        }

    }
}

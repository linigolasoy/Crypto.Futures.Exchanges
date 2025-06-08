using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc
{

    internal class MexcSymbolJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("displayName")]
        public string DisplayName { get; set; } = string.Empty;
        [JsonProperty("displayNameEn")]
        public string DisplayNameEn { get; set; } = string.Empty;
        [JsonProperty("baseCoin")]
        public string BaseCoin { get; set; } = string.Empty;
        [JsonProperty("quoteCoin")]
        public string QuoteCoin { get; set; } = string.Empty;
        [JsonProperty("contractSize")]
        public decimal ContractSize { get; set; } = 0;
        [JsonProperty("minLeverage")]
        public int MinLeverage { get; set; } = 1;
        [JsonProperty("maxLeverage")]
        public int MaxLeverage { get; set; } = 1;
        [JsonProperty("priceScale")]
        public int PriceScale { get; set; } = 1;
        [JsonProperty("volScale")]
        public int VolScale { get; set; } = 1;
        [JsonProperty("amountScale")]
        public int AmountScale { get; set; } = 1;
        [JsonProperty("priceUnit")]
        public decimal PriceUnit { get; set; } = 0;
        [JsonProperty("volUnit")]
        public decimal VolUnit { get; set; } = 0;

        [JsonProperty("minVol")]
        public decimal MinVol { get; set; } = 0;
        [JsonProperty("maxVol")]
        public decimal MaxVol { get; set; } = 0;
        [JsonProperty("takerFeeRate")]
        public decimal TakerFeeRate { get; set; } = 0;
        [JsonProperty("makerFeeRate")]
        public decimal MakerFeeRate { get; set; } = 0;

        [JsonProperty("createTime")]
        public long CreateTime { get; set; } = 0;
        [JsonProperty("openingTime")]
        public long OpeningTime { get; set; } = 0;
    }

    internal class MexcSymbol : BaseSymbol
    {

        public MexcSymbol(IFuturesExchange oExchange, MexcSymbolJson oJson):
            base(oExchange, oJson.Symbol, oJson.BaseCoin, oJson.QuoteCoin)
        {
            LeverageMin = oJson.MinLeverage;
            LeverageMax = oJson.MaxLeverage;
            FeeMaker = oJson.MakerFeeRate;
            FeeTaker = oJson.TakerFeeRate;
            Decimals = oJson.PriceScale;
            QuantityDecimals = oJson.VolScale;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.OpeningTime);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            ListDate = dDate;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static IFuturesSymbol? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            if (!(oToken is JObject)) return null;
            MexcSymbolJson? oJson = oToken.ToObject<MexcSymbolJson>();
            if (oJson == null) return null;

            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.OpeningTime);
            DateTime dDate = oOffset.Date.ToLocalTime();
            if (dDate > DateTime.Now) return null;
            return new MexcSymbol(oExchange, oJson);
        }
    }
}

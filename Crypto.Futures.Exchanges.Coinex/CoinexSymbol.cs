using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crypto.Futures.Exchanges.Coinex
{


    internal class CoinexSymbolJson
    {
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        [JsonProperty("contract_type")]
        public string ContractType { get; set; } = string.Empty;
        [JsonProperty("maker_fee_rate")]
        public string MakerFeeRate { get; set; } = string.Empty;
        [JsonProperty("taker_fee_rate")]
        public string TakerFeeRate { get; set; } = string.Empty;
        [JsonProperty("min_amount")]
        public string MinAmount { get; set; } = string.Empty;
        [JsonProperty("base_ccy")]
        public string BaseCcy { get; set; } = string.Empty;
        [JsonProperty("quote_ccy")]
        public string QuoteCcy { get; set; } = string.Empty;
        [JsonProperty("base_ccy_precision")]
        public int BaseCcyPrecision { get; set; } = 0;
        [JsonProperty("quote_ccy_precision")]
        public int QuoteCcyPrecision { get; set; } = 0;
        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;
        [JsonProperty("tick_size")]
        public string TickSize { get; set; } = string.Empty;
        [JsonProperty("leverage")]
        public List<int> Leverage { get; set; } = new List<int>();
        [JsonProperty("open_interest_volume")]
        public string OpenInterestVolume { get; set; } = string.Empty;
        [JsonProperty("is_market_available")]
        public bool IsMarketAvailable { get; set; } = false;
        [JsonProperty("is_copy_trading_available")]
        public bool IsCopyTradingAvailable { get; set; } = false;
    }
    internal class CoinexSymbol : BaseSymbol
    {
        public CoinexSymbol(IFuturesExchange exchange, CoinexSymbolJson json):
            base(exchange, json.Market, json.BaseCcy, json.QuoteCcy)
        {
            LeverageMax = json.Leverage.Max();
            LeverageMin = json.Leverage.Min();
            FeeMaker = decimal.Parse(json.MakerFeeRate);
            FeeTaker = decimal.Parse(json.TakerFeeRate);
            Decimals = json.BaseCcyPrecision;
            ContractSize = 1; // Coinex does not specify contract size, assume 1
            UseContractSize = false; // Not used
            QuantityDecimals = json.QuoteCcyPrecision;
            Minimum = decimal.Parse(json.MinAmount);
            ListDate = DateTime.Today.AddYears(-10);
        }

        public static IFuturesSymbol? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            if (!(oToken is JObject)) return null;

            CoinexSymbolJson? oJson = oToken.ToObject<CoinexSymbolJson>();
            if (oJson == null) return null;
            return new CoinexSymbol(oExchange, oJson);
        }
    }
}

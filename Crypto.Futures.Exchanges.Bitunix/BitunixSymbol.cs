using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitunix
{

    internal class BitunixSymbolJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("base")]
        public string Base { get; set; } = string.Empty;
        [JsonProperty("quote")]
        public string Quote { get; set; } = string.Empty;
        [JsonProperty("minTradeVolume")]
        public string MinTradeVolume { get; set; } = string.Empty;
        [JsonProperty("minBuyPriceOffset")]
        public string MinBuyPriceOffset { get; set; } = string.Empty;
        [JsonProperty("maxSellPriceOffset")]
        public string MaxSellPriceOffset { get; set; } = string.Empty;
        [JsonProperty("maxLimitOrderVolume")]
        public string MaxLimitOrderVolume { get; set; } = string.Empty;
        [JsonProperty("maxMarketOrderVolume")]
        public string MaxMarketOrderVolume { get; set; } = string.Empty;
        [JsonProperty("basePrecision")]
        public int BasePrecision { get; set; }
        [JsonProperty("quotePrecision")]
        public int QuotePrecision { get; set; }
        [JsonProperty("maxLeverage")]
        public int MaxLeverage { get; set; }
        [JsonProperty("minLeverage")]
        public int MinLeverage { get; set; }
        [JsonProperty("defaultLeverage")]
        public int DefaultLeverage { get; set; }
        [JsonProperty("defaultMarginMode")]
        public string DefaultMarginMode { get; set; } = string.Empty;
        [JsonProperty("priceProtectScope")]
        public string PriceProtectScope { get; set; } = string.Empty;
        [JsonProperty("symbolStatus")] // string OPEN: trade normal CANCEL_ONLY: cancel only STOP: can't open/close position
        public string SymbolStatus { get; set; } = string.Empty;
    }
    internal class BitunixSymbol : BaseSymbol
    {
        public BitunixSymbol(IFuturesExchange oExchange, BitunixSymbolJson oJson) :
            base(oExchange, oJson.Symbol, oJson.Base, oJson.Quote)
        {
            LeverageMax = oJson.MaxLeverage;
            LeverageMin = oJson.MinLeverage;

            FeeMaker = 0.0002m; // 0.02% maker fee  
            FeeTaker = 0.0006m; // 0.06% taker fee

            Decimals = oJson.QuotePrecision;
            QuantityDecimals = oJson.BasePrecision;
            ContractSize = 1; // Bitunix does not use contract size, so we set it to 1
            UseContractSize = false; // Bitunix does not use contract size, so we set it to false

            ListDate = DateTime.Today.AddYears(-1);
            Minimum = decimal.Parse(oJson.MinTradeVolume, System.Globalization.CultureInfo.InvariantCulture);
        }


        public static IFuturesSymbol? Parse( IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            BitunixSymbolJson? oJson = oToken.ToObject<BitunixSymbolJson>();
            if (oJson == null) return null;
            if (oJson.SymbolStatus.ToUpper() != "OPEN") return null;
            return new BitunixSymbol(oExchange, oJson);
        }
    }
}

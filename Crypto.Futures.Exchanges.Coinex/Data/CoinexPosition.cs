using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Crypto.Futures.Exchanges.Coinex.Data
{

    internal class CoinexPositionJson
    {
        [JsonProperty("position_id")]
        public int PositionId { get; set; }
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        [JsonProperty("market_type")]
        public string MarketType { get; set; } = string.Empty;
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;
        [JsonProperty("margin_mode")]
        public string MarginMode { get; set; } = string.Empty;
        [JsonProperty("open_interest")]
        public string OpenInterest { get; set; } = string.Empty;
        [JsonProperty("close_avbl")]
        public string CloseAvailable { get; set; } = string.Empty;
        [JsonProperty("ath_position_amount")]
        public string AthPositionAmount { get; set; } = string.Empty;
        [JsonProperty("unrealized_pnl")]
        public string UnrealizedPnl { get; set; } = string.Empty;
        [JsonProperty("realized_pnl")]
        public string RealizedPnl { get; set; } = string.Empty;
        [JsonProperty("avg_entry_price")]
        public string AverageEntryPrice { get; set; } = string.Empty;
        [JsonProperty("cml_position_value")]
        public string CmlPositionValue { get; set; } = string.Empty;
        [JsonProperty("max_position_value")]
        public string MaxPositionValue { get; set; } = string.Empty;
        [JsonProperty("take_profit_price")]
        public string TakeProfitPrice { get; set; } = string.Empty;
        [JsonProperty("stop_loss_price")]
        public string StopLossPrice { get; set; } = string.Empty;
        [JsonProperty("take_profit_type")]
        public string TakeProfitType { get; set; } = string.Empty;
        [JsonProperty("stop_loss_type")]
        public string StopLossType { get; set; } = string.Empty;
        [JsonProperty("leverage")]
        public string Leverage { get; set; } = string.Empty;
        [JsonProperty("margin_avbl")]
        public string MarginAvailable { get; set; } = string.Empty;
        [JsonProperty("ath_margin_size")]
        public string AthMarginSize { get; set; } = string.Empty;
        [JsonProperty("position_margin_rate")]
        public string PositionMarginRate { get; set; } = string.Empty;
        [JsonProperty("created_at")]
        public long CreatedAt { get; set; } = 0;
        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; } = 0;
    }
    internal class CoinexPosition : IPosition
    {
        private CoinexPosition( IFuturesSymbol oSymbol, CoinexPositionJson oJson )
        {
            Symbol = oSymbol;
            Id = oJson.PositionId.ToString();
            CreatedAt = Util.FromUnixTimestamp(oJson.CreatedAt, true);
            UpdatedAt = Util.FromUnixTimestamp(oJson.UpdatedAt, true);
            IsLong = oJson.Side.Equals("long", StringComparison.OrdinalIgnoreCase);
            IsOpen = true;
            AveragePriceOpen = decimal.Parse(oJson.AverageEntryPrice, CultureInfo.InvariantCulture);
            Quantity = decimal.Parse(oJson.AthPositionAmount, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
        }
        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; }

        public bool IsLong { get; }

        public bool IsOpen { get; }

        public decimal AveragePriceOpen { get; }

        public decimal Quantity { get; }

        public static IPosition? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            CoinexPositionJson? oJson = oToken.ToObject<CoinexPositionJson>();
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Market);
            if (oSymbol == null) return null;
            return new CoinexPosition(oSymbol, oJson);  
        }
    }
}

using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bingx.Data
{

    internal class BingxPositionJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("positionId")]
        public string PositionId { get; set; } = string.Empty;
        [JsonProperty("positionSide")]
        public string PositionSide { get; set; } = string.Empty;
        [JsonProperty("isolated")]
        public bool Isolated { get; set; }
        [JsonProperty("positionAmt")]
        public string PositionAmt { get; set; } = string.Empty;
        [JsonProperty("availableAmt")]
        public string AvailableAmt { get; set; } = string.Empty;
        [JsonProperty("unrealizedProfit")]
        public string UnrealizedProfit { get; set; } = string.Empty;
        [JsonProperty("realisedProfit")]
        public string RealisedProfit { get; set; } = string.Empty;
        [JsonProperty("initialMargin")]
        public string InitialMargin { get; set; } = string.Empty;
        [JsonProperty("margin")]
        public string Margin { get; set; } = string.Empty;
        [JsonProperty("avgPrice")]
        public string AveragePrice { get; set; } = string.Empty;
        [JsonProperty("liquidationPrice")]
        public float LiquidationPrice { get; set; }
        [JsonProperty("leverage")]
        public int Leverage { get; set; }
        [JsonProperty("positionValue")]
        public string PositionValue { get; set; } = string.Empty;
        [JsonProperty("markPrice")]
        public string MarkPrice { get; set; } = string.Empty;
        [JsonProperty("riskRate")]
        public string RiskRate { get; set; } = string.Empty;
        [JsonProperty("maxMarginReduction")]
        public string MaxMarginReduction { get; set; } = string.Empty;
        [JsonProperty("pnlRatio")]
        public string PnlRatio { get; set; } = string.Empty;
        [JsonProperty("updateTime")]
        public long UpdateTime { get; set; }
    }
    internal class BingxPosition : IPosition
    {

        internal BingxPosition(IFuturesSymbol oSymbol, BingxPositionJson oJson) 
        { 
            Id = oJson.PositionId;
            Symbol = oSymbol;
            CreatedAt = Util.FromUnixTimestamp(oJson.UpdateTime, true);
            UpdatedAt = CreatedAt; // Bingx does not provide a separate updated time, so we use created time.
            IsLong = (oJson.PositionSide.ToUpper() == "LONG");
            IsOpen = true;
            AveragePriceOpen = decimal.Parse(oJson.AveragePrice, System.Globalization.CultureInfo.InvariantCulture);
            Quantity = decimal.Parse(oJson.PositionAmt, System.Globalization.CultureInfo.InvariantCulture) * oSymbol.ContractSize;
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
            BingxPositionJson? oJson = oToken.ToObject<BingxPositionJson>();
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;

            return new BingxPosition(oSymbol, oJson);
        }
    }
}

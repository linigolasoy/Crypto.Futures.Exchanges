using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Crypto.Futures.Exchanges.Bitget.Data
{

    internal class BitgetPositionJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("marginCoin")]
        public string MarginCoin { get; set; } = string.Empty;
        [JsonProperty("holdSide")] // long: long position       short: short position
        public string HoldSide { get; set; } = string.Empty;
        [JsonProperty("openDelegateSize")]
        public string OpenDelegateSize { get; set; } = string.Empty;
        [JsonProperty("marginSize")]
        public string MarginSize { get; set; } = string.Empty;
        [JsonProperty("available")]
        public string Available { get; set; } = string.Empty;
        [JsonProperty("locked")]
        public string Locked { get; set; } = string.Empty;
        [JsonProperty("total")]
        public string Total { get; set; } = string.Empty;
        [JsonProperty("leverage")]
        public string Leverage { get; set; } = string.Empty;
        [JsonProperty("achievedProfits")]
        public string AchievedProfits { get; set; } = string.Empty;
        [JsonProperty("openPriceAvg")]
        public string OpenPriceAvg { get; set; } = string.Empty;
        [JsonProperty("marginMode")] // isolated: isolated margin crossed: cross margin
        public string MarginMode { get; set; } = string.Empty; // isolated: isolated margin, crossed: cross margin
        [JsonProperty("posMode")] // one_way_mode: positions in one-way mode, hedge_mode: positions in hedge-mode
        public string PosMode { get; set; } = string.Empty; // one_way_mode: positions in one-way mode, hedge_mode: positions in hedge-mode
        [JsonProperty("unrealizedPL")]
        public string UnrealizedPL { get; set; } = string.Empty;
        [JsonProperty("liquidationPrice")]
        public string LiquidationPrice { get; set; } = string.Empty;
        [JsonProperty("keepMarginRate")]
        public string KeepMarginRate { get; set; } = string.Empty;
        [JsonProperty("markPrice")]
        public string MarkPrice { get; set; } = string.Empty;
        [JsonProperty("marginRatio")]
        public string MarginRatio { get; set; } = string.Empty;
        [JsonProperty("breakEvenPrice")]
        public string BreakEvenPrice { get; set; } = string.Empty;
        [JsonProperty("totalFee")]
        public string TotalFee { get; set; } = string.Empty;
        [JsonProperty("takeProfit")]
        public string TakeProfit { get; set; } = string.Empty;
        [JsonProperty("stopLoss")]
        public string StopLoss { get; set; } = string.Empty;
        [JsonProperty("takeProfitId")]
        public string TakeProfitId { get; set; } = string.Empty;
        [JsonProperty("stopLossId")]
        public string StopLossId { get; set; } = string.Empty;
        [JsonProperty("deductedFee")]
        public string DeductedFee { get; set; } = string.Empty;
        [JsonProperty("cTime")]
        public string CTime { get; set; } = string.Empty; // Creation time, timestamp, milliseconds
        [JsonProperty("uTime")]
        public string UTime { get; set; } = string.Empty; // Last updated time, timestamp, milliseconds
        
    }
    internal class BitgetPosition : IPosition
    {
        private BitgetPosition(IFuturesSymbol oSymbol, BitgetPositionJson oJson )
        {
            Id = oJson.CTime;
            Symbol = oSymbol;
            CreatedAt = Util.FromUnixTimestamp(oJson.CTime, true);
            UpdatedAt = Util.FromUnixTimestamp(oJson.UTime, true);
            IsLong = oJson.HoldSide.Equals("long", StringComparison.CurrentCultureIgnoreCase);
            IsOpen = true;
            AveragePriceOpen = decimal.Parse(oJson.OpenPriceAvg, CultureInfo.InvariantCulture);
            Quantity = decimal.Parse(oJson.Total, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
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
            BitgetPositionJson? oJson = oToken.ToObject<BitgetPositionJson>();
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;
            return new BitgetPosition(oSymbol, oJson);
        }
    }
}

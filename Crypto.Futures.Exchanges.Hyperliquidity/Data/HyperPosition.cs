using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Data
{
    internal class HyperPosition : IPosition
    {
        public HyperPosition(IFuturesSymbol oSymbol, HyperLiquidPosition oPos) 
        {
            // MERDEMERDE Id = oPos.Position.
            Symbol = oSymbol;
            IsLong = (oPos.Position.PositionQuantity != null && oPos.Position.PositionQuantity.Value > 0);
            IsOpen = true;
            AveragePriceOpen = (oPos.Position.AverageEntryPrice == null ? 0 : oPos.Position.AverageEntryPrice.Value);
            Quantity = Math.Abs( (oPos.Position.PositionQuantity == null ? 0 : oPos.Position.PositionQuantity.Value) );
            Profit = (oPos.Position.UnrealizedPnl == null ? 0 : oPos.Position.UnrealizedPnl.Value);
            decimal nEntryPrice = (oPos.Position.AverageEntryPrice == null ? 0 : oPos.Position.AverageEntryPrice.Value);
            string strId = $"{oSymbol.Symbol}_{IsLong.ToString()}_{Quantity.ToString()}_{nEntryPrice.ToString()}";
            Id = strId;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;    

        }

        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public bool IsLong { get; }

        public bool IsOpen { get; private set; } = true;

        public decimal AveragePriceOpen { get; }

        public decimal? PriceClose { get; set; } = null;

        public decimal Quantity { get; }

        public decimal Profit { get; set; } = 0;

        public WsMessageType MessageType { get => WsMessageType.Position; }

        public void Close(decimal nPriceClose)
        {
            PriceClose = nPriceClose;
            IsOpen = false;
            UpdatedAt = DateTime.Now;
            Profit = (PriceClose.Value - AveragePriceOpen) * Quantity * (IsLong ? 1 : -1);
        }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IPosition)) return;
            IPosition oPos = (IPosition)oMessage;
            PriceClose = oPos.PriceClose;
            IsOpen = oPos.IsOpen;
            UpdatedAt = DateTime.Now;
            Profit = oPos.Profit;
        }
    }
}

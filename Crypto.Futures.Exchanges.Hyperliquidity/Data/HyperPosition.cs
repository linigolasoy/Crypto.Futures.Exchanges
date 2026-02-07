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
            Symbol = oSymbol;
            IsLong = (oPos.Position.PositionQuantity != null && oPos.Position.PositionQuantity.Value > 0);
            IsOpen = true;
            AveragePriceOpen = (oPos.Position.AverageEntryPrice == null ? 0 : oPos.Position.AverageEntryPrice.Value);
            Quantity = Math.Abs( (oPos.Position.PositionQuantity == null ? 0 : oPos.Position.PositionQuantity.Value) );
            Profit = (oPos.Position.UnrealizedPnl == null ? 0 : oPos.Position.UnrealizedPnl.Value);
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;    
        }

        public string Id => throw new NotImplementedException();

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; }

        public bool IsLong { get; }

        public bool IsOpen { get; }

        public decimal AveragePriceOpen { get; }

        public decimal? PriceClose { get; set; } = null;

        public decimal Quantity { get; }    

        public decimal Profit { get;}

        public WsMessageType MessageType { get => WsMessageType.Position; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}

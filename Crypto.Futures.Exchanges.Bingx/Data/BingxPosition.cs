using BingX.Net.Enums;
using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
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

    internal class BingxPosition : IPosition
    {

        internal BingxPosition(IFuturesSymbol oSymbol, BingXPosition oJson) 
        {
            Id = oJson.PositionId;
            Symbol = oSymbol;
            CreatedAt = (oJson.UpdateTime == null ? DateTime.Now : oJson.UpdateTime.Value);
            UpdatedAt = CreatedAt; // Bingx does not provide a separate updated time, so we use created time.
            IsLong = (oJson.Side == BingX.Net.Enums.TradeSide.Long);
            IsOpen = true;
            AveragePriceOpen = oJson.AveragePrice;
            Quantity = oJson.Size;
        }


        internal BingxPosition(IFuturesSymbol oSymbol, BingXFuturesPositionChange oChange)
        {
            Id = oSymbol.Symbol;
            Symbol = oSymbol;
            CreatedAt = DateTime.Now;
            UpdatedAt = CreatedAt; // Bingx does not provide a separate updated time, so we use created time.
            IsLong = (oChange.Side == TradeSide.Long);
            IsOpen = (oChange.Size > 0);
            AveragePriceOpen = oChange.EntryPrice;
            PriceClose = oChange.EntryPrice;    
            Quantity = oChange.Size;

            Profit = oChange.UnrealizedPnl + (oChange.RealizedPnl == null? 0 : oChange.RealizedPnl.Value);


        }
        internal BingxPosition(IFuturesSymbol oSymbol, BingXPositionHistory oJson)
        {
            Id = oJson.PositionId;
            Symbol = oSymbol;
            CreatedAt = oJson.OpenTime.ToLocalTime();
            UpdatedAt = oJson.UpdateTime.ToLocalTime(); // Bingx does not provide a separate updated time, so we use created time.
            IsLong = (oJson.PositionSide == PositionSide.Long);
            IsOpen = true;
            AveragePriceOpen = oJson.AveragePrice;
            Quantity = oJson.PositionQuantity;
            PriceClose = oJson.AverageClosePrice;
        }

        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; }

        public bool IsLong { get; }

        public bool IsOpen { get; internal set; }

        public decimal AveragePriceOpen { get; }
        public decimal? PriceClose { get; set; } = null;

        public decimal Profit { get; private set; } = 0;
        public decimal Quantity { get; }

        public WsMessageType MessageType { get => WsMessageType.Position; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if( !(oMessage is IPosition)) return;
            IPosition oPosition = (IPosition)oMessage;
            IsOpen = oPosition.IsOpen;  
            PriceClose= oPosition.PriceClose;   
            Profit = oPosition.Profit;
        }
    }
}

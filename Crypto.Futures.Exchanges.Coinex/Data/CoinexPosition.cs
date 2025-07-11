using CoinEx.Net.Enums;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Data
{
    internal class CoinexPosition : IPosition
    {

        public CoinexPosition( IFuturesSymbol oSymbol, CoinExPosition oJson)
        {
            // Constructor logic if needed
            Id = oJson.Id.ToString();
            Symbol = oSymbol;
            CreatedAt = oJson.CreateTime.ToLocalTime();
            UpdatedAt = (oJson.UpdateTime ==null? DateTime.Now: oJson.UpdateTime.Value.ToLocalTime());
            IsLong = (oJson.Side == PositionSide.Long);
            IsOpen = true;
            AveragePriceOpen = oJson.AverageEntryPrice;
            Quantity = oJson.AthPositionQuantity;
            PriceClose = oJson.SettlePrice;
            Profit = oJson.UnrealizedPnl + oJson.RealizedPnl;
        }

        public CoinexPosition(IFuturesSymbol oSymbol, CoinExPositionUpdate oUpdate)
        {
            // Constructor logic if needed
            Id = oUpdate.Position.Id.ToString();
            Symbol = oSymbol;
            CreatedAt = oUpdate.Position.CreateTime.ToLocalTime();
            UpdatedAt = (oUpdate.Position.UpdateTime == null ? DateTime.Now : oUpdate.Position.UpdateTime.Value.ToLocalTime());
            IsLong = (oUpdate.Position.Side == PositionSide.Long);
            IsOpen = (oUpdate.Event != PositionUpdateType.Liquidation && oUpdate.Event != PositionUpdateType.SystemClose && oUpdate.Event != PositionUpdateType.Close);
            AveragePriceOpen = oUpdate.Position.AverageEntryPrice;
            Quantity = oUpdate.Position.AthPositionQuantity;
            PriceClose = oUpdate.Position.SettlePrice;
            Profit = oUpdate.Position.UnrealizedPnl + oUpdate.Position.RealizedPnl;
        }

        public string Id { get; }
        public WsMessageType MessageType { get => WsMessageType.Position; }

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public bool IsLong { get; }

        public bool IsOpen { get; private set; }

        public decimal AveragePriceOpen { get; }
        public decimal? PriceClose { get; set; }= null; 

        public decimal Quantity { get; }
        public decimal Profit { get; private set; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (oMessage == null) return;
            if( !(oMessage is IPosition)) return;
            IPosition oPoisition = (IPosition)oMessage; 
            UpdatedAt = oPoisition.UpdatedAt;   
            IsOpen = oPoisition.IsOpen; 
            Profit= oPoisition.Profit;
            PriceClose= oPoisition.PriceClose;  
        }
    }
}

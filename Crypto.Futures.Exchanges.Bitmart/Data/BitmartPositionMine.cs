using BitMart.Net.Enums;
using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{
    internal class BitmartPositionMine: IPosition
    {
        public BitmartPositionMine(IFuturesSymbol oSymbol, BitMartPosition oJson)
        {
            Id = Guid.NewGuid().ToString();
            Symbol = oSymbol;
            CreatedAt = (oJson.OpenTime == null ? DateTime.Now: oJson.OpenTime.Value.ToLocalTime());
            UpdatedAt = (oJson.Timestamp.ToLocalTime());
            IsLong = (oJson.PositionSide == PositionSide.Long );
            IsOpen = true;
            AveragePriceOpen = (oJson.OpenAveragePrice == null? 0: oJson.OpenAveragePrice.Value);
            Quantity = (oJson.CurrentQuantity == null? 0 : oJson.CurrentQuantity.Value) * oSymbol.ContractSize;
        }
        public BitmartPositionMine(IFuturesSymbol oSymbol, BitMartPositionUpdate oUpdate)
        {
            Id = oSymbol.Symbol;
            Symbol = oSymbol;
            CreatedAt = oUpdate.CreateTime.ToLocalTime();
            UpdatedAt = ( oUpdate.UpdateTime == null ? CreatedAt : oUpdate.UpdateTime.Value.ToLocalTime());
            IsLong = (oUpdate.PositionSide == PositionSide.Long);
            AveragePriceOpen = (oUpdate.AverageOpenPrice == null ? 0 : oUpdate.AverageOpenPrice.Value);
            Quantity = oUpdate.PositionSize * oSymbol.ContractSize;
            decimal? nPrice = (oUpdate.AverageClosePrice == null ? oUpdate.AverageHoldPrice: oUpdate.AverageClosePrice.Value);
            if (nPrice == null) nPrice = AveragePriceOpen;
            Profit = (IsLong ? 1.0M : -1.0M) * ( nPrice.Value - AveragePriceOpen ) * Quantity;
            PriceClose = oUpdate.AverageClosePrice;

            IsOpen = (oUpdate.PositionSize > 0);
        }

        public BitmartPositionMine(IFuturesSymbol oSymbol, BitMartFuturesUserTrade oOpenTrade, BitMartFuturesUserTrade oCloseTrade)
        {
            Id = Guid.NewGuid().ToString();
            Symbol = oSymbol;
            CreatedAt = oOpenTrade.CreateTime.ToLocalTime();
            UpdatedAt = oCloseTrade.CreateTime.ToLocalTime();
            IsLong = (oOpenTrade.Side == FuturesSide.SellCloseLong || oOpenTrade.Side == FuturesSide.BuyOpenLong);
            IsOpen = true;
            AveragePriceOpen = oOpenTrade.Price;
            Quantity = oOpenTrade.Quantity * oSymbol.ContractSize;
            PriceClose = oCloseTrade.Price;
        }

        public WsMessageType MessageType { get => WsMessageType.Position; }
        public string Id { get; }
        public decimal Profit { get; private set; } = 0;
        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public bool IsLong { get; }

        public bool IsOpen { get; internal set; }

        public decimal AveragePriceOpen { get; }
        public decimal? PriceClose { get; set; } = null;

        public decimal Quantity { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IPosition)) return;
            IPosition oPosition = (IPosition)oMessage;  
            UpdatedAt = oPosition.UpdatedAt;
            IsOpen = oPosition.IsOpen;
            PriceClose = oPosition.PriceClose;
            Profit = oPosition.Profit;
        }
    }
}

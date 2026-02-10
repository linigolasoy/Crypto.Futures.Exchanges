using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Data
{
    internal class HyperOrder : IOrder
    {

        public HyperOrder(IFuturesSymbol oSymbol, HyperLiquidOrderStatus oStatus) : this(oSymbol, oStatus.Order)
        {
            switch( oStatus.Status)
            {
                case OrderStatus.Open:
                    Status = ModelOrderStatus.New;
                    break;
                case OrderStatus.WaitingFill:
                    Status = ModelOrderStatus.PartiallyFilled;
                    break;
                case OrderStatus.Filled:
                    Status = ModelOrderStatus.Filled;
                    break;
                case OrderStatus.Rejected:
                case OrderStatus.Canceled:
                    Status = ModelOrderStatus.Canceled;
                    break;
                default:
                    break;
            }
        }

        public HyperOrder(IFuturesSymbol oSymbol, HyperLiquidOpenOrder oOrder)
        {
            Symbol = oSymbol;
            OrderId = oOrder.OrderId.ToString();
            Side = (oOrder.OrderSide == OrderSide.Buy ? ModelOrderSide.Buy : ModelOrderSide.Sell);
            Type = ModelOrderType.Limit;
            if( this.Type != ModelOrderType.Market)
            {
                Price = oOrder.Price;
            }
            Quantity = oOrder.Quantity;
            Filled = Quantity - oOrder.QuantityRemaining;
            if( oOrder.QuantityRemaining <= 0 )
            {
                Status = ModelOrderStatus.Filled;
            }
            if( oOrder.QuantityRemaining < oOrder.Quantity)
            {
                Status = ModelOrderStatus.PartiallyFilled;
            }
            CreatedAt = DateTime.Now;
            UpdatedAt = oOrder.Timestamp.ToLocalTime();
        }
        public HyperOrder(IFuturesSymbol oSymbol, HyperLiquidOrder oOrder)
        {
            Symbol = oSymbol;
            OrderId = oOrder.OrderId.ToString();
            Side = (oOrder.OrderSide == OrderSide.Buy ? ModelOrderSide.Buy : ModelOrderSide.Sell);
            switch (oOrder.OrderType)
            {
                case OrderType.Limit:
                    Type = ModelOrderType.Limit;
                    break;
                case OrderType.Market:
                    Type = ModelOrderType.Market;
                    break;
                default:
                    Type = ModelOrderType.Stop;
                    break;
            }
            if (this.Type != ModelOrderType.Market)
            {
                Price = oOrder.Price;
            }
            Quantity = oOrder.Quantity;
            Filled = Quantity - oOrder.QuantityRemaining;
            if (oOrder.QuantityRemaining <= 0)
            {
                Status = ModelOrderStatus.Filled;
            }
            if (oOrder.QuantityRemaining < oOrder.Quantity)
            {
                Status = ModelOrderStatus.PartiallyFilled;
            }
            CreatedAt = DateTime.Now;
            UpdatedAt = oOrder.Timestamp.ToLocalTime();
        }


        public string OrderId { get; }

        public IFuturesSymbol Symbol { get; }

        public ModelOrderStatus Status { get; private set; } = ModelOrderStatus.New;

        public ModelOrderType Type { get; }

        public ModelOrderSide Side { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public decimal Quantity { get; }

        public decimal? Price { get; }

        public decimal Filled { get; private set; }

        public decimal FilledPrice { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Order; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IOrder)) return;
            IOrder oOrder = (IOrder)oMessage;
            Status = oOrder.Status;
            UpdatedAt = oOrder.UpdatedAt;
            Filled = oOrder.Filled;
            FilledPrice = oOrder.FilledPrice;

        }
    }
}

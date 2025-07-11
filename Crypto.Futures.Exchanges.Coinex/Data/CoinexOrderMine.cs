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
    internal class CoinexOrderMine : IOrder
    {


        public CoinexOrderMine( IFuturesSymbol oSymbol, CoinExFuturesOrderUpdate oUpdate) 
        {
            CoinExFuturesOrder oOrder = oUpdate.Order;
            Symbol = oSymbol;

            OrderId = oOrder.Id.ToString();
            Side = (oOrder.Side == OrderSide.Buy ? ModelOrderSide.Buy : ModelOrderSide.Sell);

            switch( oOrder.OrderType) 
            {
                case OrderTypeV2.Market:
                    Type = ModelOrderType.Market; break;
                case OrderTypeV2.Limit:
                    Type = ModelOrderType.Limit; break;
                default:
                    throw new NotImplementedException();    
            }
            Quantity = oOrder.Quantity * oSymbol.ContractSize;
            Price = oOrder.Price;
            decimal nRemaining = oOrder.QuantityRemaining * oSymbol.ContractSize;
            Filled = (Quantity - nRemaining);
            CreatedAt = oOrder.CreateTime.ToLocalTime();
            UpdatedAt = (oOrder.UpdateTime == null ? CreatedAt : oOrder.UpdateTime.Value.ToLocalTime());
            Status = PutStatus(oOrder.Status, oUpdate.Event, nRemaining);
        }

        private ModelOrderStatus PutStatus(OrderStatusV2? eCoinStatus, OrderUpdateType eUpdateType, decimal nRemaining )
        {
            if (eCoinStatus == null)
            {
                switch( eUpdateType )
                {
                    case OrderUpdateType.Put:
                        return ModelOrderStatus.New;
                    case OrderUpdateType.Update:
                        return ModelOrderStatus.Unknown;
                    case OrderUpdateType.Finish:
                        return ( nRemaining > 0 ? ModelOrderStatus.Canceled : ModelOrderStatus.Filled);
                    case OrderUpdateType.Edit:
                        return ModelOrderStatus.Unknown;
                }
                return ModelOrderStatus.Unknown;
            }
            switch (eCoinStatus)
            {
                case OrderStatusV2.Open:
                    return ModelOrderStatus.New;
                case OrderStatusV2.PartiallyFilled:
                    return ModelOrderStatus.PartiallyFilled;
                case OrderStatusV2.Filled:
                    return ModelOrderStatus.Filled;
                case OrderStatusV2.PartiallyCanceled:
                case OrderStatusV2.Canceled:
                    return ModelOrderStatus.Canceled;
                default: 
                    return ModelOrderStatus.Unknown;
            }

        }

        public string OrderId { get; }

        public IFuturesSymbol Symbol { get; }

        public ModelOrderStatus Status { get; private set; }

        public ModelOrderType Type { get; }

        public ModelOrderSide Side { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }

        public decimal Quantity { get; }

        public decimal? Price { get; }

        public decimal Filled { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Order; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (oMessage == null || !(oMessage is IOrder)) return;
            IOrder oOrder = (IOrder)oMessage;
            UpdatedAt = oOrder.UpdatedAt;
            if (oOrder.Status != ModelOrderStatus.Unknown && oOrder.Status != Status)
            {
                ModelOrderStatus eStatus = oOrder.Status;
                /*
                if( oOrder.Status == ModelOrderStatus.Canceled )
                {
                    if (oOrder.Filled >= oOrder.Quantity) eStatus = ModelOrderStatus.Filled;
                }
                */
                Status = eStatus;
            }
            Filled = oOrder.Filled;

        }
    }
}

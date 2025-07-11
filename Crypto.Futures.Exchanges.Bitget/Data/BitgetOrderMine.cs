using Bitget.Net.Enums.V2;
using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    internal class BitgetOrderMine : IOrder
    {
        public BitgetOrderMine( IFuturesSymbol oSymbol, BitgetFuturesOrderUpdate oUpdate) 
        { 
            Symbol = oSymbol;
            OrderId = oUpdate.OrderId;
            Quantity = oUpdate.Quantity * oSymbol.ContractSize;
            Price = oUpdate.Price;
            Status = GetStatus(oUpdate.Status);
            Type = (oUpdate.OrderType == OrderType.Limit ? ModelOrderType.Limit : ModelOrderType.Market);
            Side = (oUpdate.Side == OrderSide.Buy ? ModelOrderSide.Buy : ModelOrderSide.Sell);

            CreatedAt = oUpdate.CreateTime.ToLocalTime();
            UpdatedAt = (oUpdate.UpdateTime == null ? CreatedAt : oUpdate.UpdateTime.Value.ToLocalTime());
            Filled = oUpdate.QuantityFilled;
        }

        private ModelOrderStatus GetStatus( OrderStatus  eStatus )
        {
            switch( eStatus )
            {
                case OrderStatus.Initial:
                case OrderStatus.New:
                    return ModelOrderStatus.New;
                case OrderStatus.Live:
                    return ModelOrderStatus.Placed;
                case OrderStatus.PartiallyFilled:
                    return ModelOrderStatus.PartiallyFilled;
                case OrderStatus.Filled:
                    return ModelOrderStatus.Filled;
                case OrderStatus.Canceled:
                case OrderStatus.Rejected:
                    return ModelOrderStatus.Canceled;
            }
            throw new NotImplementedException();
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
            if (!(oMessage is IOrder)) return;
            IOrder oOrder = (IOrder)oMessage;
            Status = oOrder.Status;
            UpdatedAt = oOrder.UpdatedAt;
            Filled = oOrder.Filled;

        }
    }
}

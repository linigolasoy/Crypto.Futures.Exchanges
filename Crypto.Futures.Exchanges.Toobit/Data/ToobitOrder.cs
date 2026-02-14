using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Toobit.Net.Enums;
using Toobit.Net.Objects.Models;

namespace Crypto.Futures.Exchanges.Toobit.Data
{
    /// <summary>
    /// Order on toobit
    /// </summary>
    internal class ToobitOrderMine : IOrder
    {

        public ToobitOrderMine( IFuturesSymbol oSymbol, ToobitFuturesOrderUpdate oUpdate) 
        { 
            Symbol = oSymbol;
            OrderId = oUpdate.OrderId.ToString();
            Status = PutStatus(oUpdate);
            Side = ( oUpdate.OrderSide == OrderSide.Buy ? ModelOrderSide.Buy : ModelOrderSide.Sell );
            Type = (oUpdate.OrderType == FuturesUpdateOrderType.Market ? ModelOrderType.Market : ModelOrderType.Limit);
            CreatedAt = oUpdate.CreateTime.ToLocalTime();
            UpdatedAt = oUpdate.UpdateTime.ToLocalTime();

            Price = oUpdate.Price;
            FilledPrice = (oUpdate.LastFillPrice == null ? 0 : oUpdate.LastFillPrice.Value);
            Filled = ((decimal)oUpdate.QuantityFilled * oSymbol.ContractSize);



            decimal nQuantity = (decimal)oUpdate.Quantity * oSymbol.ContractSize;
            Quantity = nQuantity;

        }

        private ModelOrderStatus PutStatus(ToobitFuturesOrderUpdate oUpdate)
        {
            switch(oUpdate.Status)
            {
                case OrderStatus.New:
                    return ModelOrderStatus.New;
                case OrderStatus.PartiallyFilled:
                    return ModelOrderStatus.PartiallyFilled;
                case OrderStatus.Filled:
                    return ModelOrderStatus.Filled;
                case OrderStatus.Canceled:
                case OrderStatus.PendingCancel:
                case OrderStatus.Rejected:
                case OrderStatus.PartiallyCanceled:
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

        public decimal Quantity { get; private set; }

        public decimal? Price { get; private set; }

        public decimal Filled { get; private set; }

        public decimal FilledPrice { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Order; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if( !(oMessage is IOrder)) return;
            IOrder oOrder = (IOrder)oMessage;   
            UpdatedAt = oOrder.UpdatedAt;
            Quantity = oOrder.Quantity;
            Price = oOrder.Price;
            Filled = oOrder.Filled;
            FilledPrice = oOrder.FilledPrice;
            Status = oOrder.Status;

        }
    }
}

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
    internal class BitmarOrderMine : IOrder
    {

        public BitmarOrderMine(IFuturesSymbol oSymbol, BitMartFuturesOrderUpdateEvent oUpdate) 
        { 
            OrderId = oUpdate.Order.OrderId;
            Symbol = oSymbol;
            Status = GetStatus(oUpdate);
            Quantity = oUpdate.Order.Quantity * oSymbol.ContractSize;
            Price = oUpdate.Order.Price;
            CreatedAt = oUpdate.Order.CreateTime.ToLocalTime();
            UpdatedAt = (oUpdate.Order.UpdateTime == null ? CreatedAt : oUpdate.Order.UpdateTime.Value.ToLocalTime());
            Side = (oUpdate.Order.Side == FuturesSide.BuyCloseShort || oUpdate.Order.Side == FuturesSide.BuyOpenLong ? ModelOrderSide.Buy : ModelOrderSide.Sell);
            ModelOrderType eType = ModelOrderType.Market;
            switch (oUpdate.Order.OrderType)
            {
                case FuturesOrderType.Market:
                    eType = ModelOrderType.Market; break;
                case FuturesOrderType.Limit:
                    eType = ModelOrderType.Limit; break;
                default:
                    throw new NotImplementedException();
            }
            Type = eType;
            Filled = oUpdate.Order.QuantityFilled * oSymbol.ContractSize;
            FilledPrice = (oUpdate.Order.AveragePrice == null ? 0 : oUpdate.Order.AveragePrice.Value);  
        }

        private ModelOrderStatus GetStatus(BitMartFuturesOrderUpdateEvent oUpdate)
        {
            if (oUpdate.Event == OrderEvent.Trade) return ModelOrderStatus.Filled;
            if( oUpdate.Event == OrderEvent.Cancel || oUpdate.Event == OrderEvent.LiquidationCancel || oUpdate.Event == OrderEvent.AdlCancel) return ModelOrderStatus.Canceled;
            switch (oUpdate.Order.Status)
            {
                case FuturesOrderStatus.Finish:
                    break;
                case FuturesOrderStatus.Approval:
                    break;
                case FuturesOrderStatus.Check:
                    break;
            }
            return ModelOrderStatus.Placed;
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

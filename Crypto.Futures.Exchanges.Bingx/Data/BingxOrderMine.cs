using BingX.Net.Enums;
using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Data
{
    internal class BingxOrderMine : IOrder
    {

        public BingxOrderMine( IFuturesSymbol oSymbol, BingXFuturesOrderUpdate oUpdate) 
        {
            Symbol = oSymbol;
            OrderId = oUpdate.OrderId.ToString();
            Status = GetStatus(oUpdate);    

            ModelOrderType eType = ModelOrderType.Market;
            switch (oUpdate.Type)
            {
                case FuturesOrderType.Market:
                    eType = ModelOrderType.Market; break;
                case FuturesOrderType.Limit:
                    eType = ModelOrderType.Limit; break;
                default:
                    throw new NotImplementedException();    
            }
            Type = eType;
            Side = (oUpdate.Side == OrderSide.Buy ? ModelOrderSide.Buy : ModelOrderSide.Sell);
            Quantity = ( oUpdate.Quantity == null ? 0 : oUpdate.Quantity.Value) * oSymbol.ContractSize;
            Price = oUpdate.Price;
            CreatedAt = (oUpdate.UpdateTime == null ? DateTime.Now : oUpdate.UpdateTime.Value.ToLocalTime());
            UpdatedAt = CreatedAt;
            Filled = (oUpdate.QuantityFilled == null ? 0 : oUpdate.QuantityFilled.Value) * oSymbol.ContractSize;
            FilledPrice = (oUpdate.AveragePrice == null ? 0 : oUpdate.AveragePrice.Value);

        }

        private ModelOrderStatus GetStatus(BingXFuturesOrderUpdate oUpdate)
        {
            switch( oUpdate.Status )
            {
                case OrderStatus.Failed:
                case OrderStatus.Canceled:
                    return ModelOrderStatus.Canceled;
                case OrderStatus.Pending:
                    return ModelOrderStatus.Placed;
                case OrderStatus.New:
                    return ModelOrderStatus.New;
                case OrderStatus.Filled:
                    return ModelOrderStatus.Filled;
                case OrderStatus.PartiallyFilled:
                    return ModelOrderStatus.PartiallyFilled;
                default: throw new NotImplementedException();   
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
        public decimal FilledPrice { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Order; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if(!(oMessage is IOrder)) return;
            IOrder oOrder = (IOrder)oMessage;   
            UpdatedAt = oOrder.UpdatedAt;   
            Status = oOrder.Status;
            Filled = oOrder.Filled; 
            FilledPrice = oOrder.FilledPrice;
        }
    }
}

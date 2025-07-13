using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    public enum ModelOrderStatus
    {
        Unknown,
        New,
        Canceled,
        Filled,
        PartiallyFilled,
        Placed
    }

    public enum ModelOrderType
    {
        Market,
        Limit,
        Stop
    }

    public enum ModelOrderSide
    {
        Buy,
        Sell
    }
    public interface IOrder:IWebsocketMessageBase
    {
        public string OrderId { get; }
        public IFuturesSymbol Symbol { get; }

        public ModelOrderStatus Status { get; } 
        public ModelOrderType Type { get; }

        public ModelOrderSide Side { get; }

        public DateTime CreatedAt { get; }  
        public DateTime UpdatedAt { get; }
        public decimal Quantity { get; }    
        public decimal? Price { get; }
        public decimal Filled { get; }
        public decimal FilledPrice { get; }
    }
}

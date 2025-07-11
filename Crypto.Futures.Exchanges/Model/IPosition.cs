using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    /// <summary>
    /// Position on a futures exchange. 
    /// </summary>
    public interface IPosition: IWebsocketMessageBase
    {
        public string Id { get; }
        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }
        public bool IsLong { get; }
        public bool IsOpen { get; }
        public decimal AveragePriceOpen { get; }
        public decimal? PriceClose { get; set; } 
        public decimal Quantity { get; }

        public decimal Profit { get; }  
    }
}

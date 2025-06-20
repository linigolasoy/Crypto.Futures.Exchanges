using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    /// <summary>
    /// Last orderbook price
    /// </summary>
    public interface IOrderbookPrice: IWebsocketMessage
    {
        public DateTime DateTime { get; }

        public decimal AskPrice { get; }
        public decimal AskVolume { get; }
        public decimal BidPrice { get; }
        public decimal BidVolume { get; }
    }
}

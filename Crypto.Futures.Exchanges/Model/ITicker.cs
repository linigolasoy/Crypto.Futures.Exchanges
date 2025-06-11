using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{

    /// <summary>
    /// Ticker data for a symbol
    /// </summary>
    public interface ITicker: IWebsocketMessage
    {
        public DateTime DateTime { get; }
        public decimal LastPrice { get; }
        public decimal AskPrice { get; }
        public decimal BidPrice { get; }
        public decimal AskVolume { get; }
        public decimal BidVolume { get; }
    }
}

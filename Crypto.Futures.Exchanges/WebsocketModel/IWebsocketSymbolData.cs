using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Data stored on websocket for each symbol
    /// </summary>
    public interface IWebsocketSymbolData
    {
        public IFuturesSymbol Symbol { get; }

        public ITrade? LastTrade { get; }   

        public IFundingRate? FundingRate { get; }   
    }
}

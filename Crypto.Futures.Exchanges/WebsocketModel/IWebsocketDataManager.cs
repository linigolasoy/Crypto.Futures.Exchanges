using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Storage of market websocket data received
    /// </summary>
    public interface IWebsocketDataManager
    {
        public IFuturesExchange Exchange { get; }

        public IWebsocketSymbolData? GetData(string strSymbol);
        public IWebsocketSymbolData? GetData(IFuturesSymbol oSymbol);

        public DateTime LastUpdate { get; } 
        public void Put(IWebsocketMessage oMessage);
    }
}

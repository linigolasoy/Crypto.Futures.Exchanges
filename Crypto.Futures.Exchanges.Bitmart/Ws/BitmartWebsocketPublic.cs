using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Ws
{
    /// <summary>
    /// Bitmart websocket
    /// </summary>
    internal class BitmartWebsocketPublic : BaseWebsocketManager, IWebsocketPublic
    {
        private const string URL_WS = "wss://openapi-ws-v2.bitmart.com/api?protocol=1.1";

        public BitmartWebsocketPublic(IFuturesMarket oMarket) : base(oMarket, URL_WS, new BitmartWebsocketParser(oMarket.Exchange))
        {
        }

    }
}

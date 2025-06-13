using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Ws
{
    /// <summary>
    /// Bitget websocket
    /// </summary>
    internal class BitgetWebsocketPublic : BaseWebsocketPublic, IWebsocketPublic
    {
        private const string URL_WS = "wss://ws.bitget.com/v2/ws/public";

        public BitgetWebsocketPublic(IFuturesMarket oMarket) : base(oMarket, URL_WS, new BitgetWebsocketParser(oMarket.Exchange))
        {
        }

    }
}

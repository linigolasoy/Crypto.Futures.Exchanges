using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace Crypto.Futures.Exchanges.Mexc.Ws
{

    /// <summary>
    /// Local websockets for Mexc
    /// </summary>
    internal class MexcWebsocketPublic : BaseWebsocketPublic, IWebsocketPublic
    {
        private const string URL_WS = "wss://contract.mexc.com/edge";

        public MexcWebsocketPublic( IFuturesMarket oMarket) : base(oMarket, URL_WS, new MexcWebsocketParser(oMarket.Exchange))
        {
        }

    }
}

using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Ws
{
    /// <summary>
    /// Local websockets for Coinex
    /// </summary>
    internal class CoinexWebsocketPublic : BaseWebsocketPublic, IWebsocketPublic
    {
        private const string URL_WS = "wss://socket.coinex.com/v2/futures";

        public CoinexWebsocketPublic(IFuturesMarket oMarket) : base(oMarket, URL_WS, new CoinexWebsocketParser(oMarket.Exchange))
        {
        }

    }
}

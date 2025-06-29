using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Ws
{
    internal class BlofinWebsocketPublic : BaseWebsocketManager, IWebsocketPublic
    {
        private const string URL_WS = "wss://openapi.blofin.com/ws/public";
        public BlofinWebsocketPublic(IFuturesMarket oMarket) : base(oMarket, URL_WS, new BlofinWebsocketParser(oMarket.Exchange))
        {
        }
    }
}

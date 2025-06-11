using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{

    public enum WsMessageType
    {
        Trade,
        FundingRate,
        KLine,
        Ticker
    }
    /// <summary>
    /// Websocket message
    /// </summary>
    public interface IWebsocketMessage
    {
        public WsMessageType MessageType { get; }
        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage);
    }
}

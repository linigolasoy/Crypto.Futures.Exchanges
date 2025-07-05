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
        Ping,
        Pong,
        Trade,
        FundingRate,
        KLine,
        Ticker,
        OrderbookPrice,
        LastPrice,
        Subscription,
        Balance,
        Position,
        Order
        
    }

    /// <summary>
    /// Message base
    /// </summary>
    public interface IWebsocketMessageBase
    {
        public WsMessageType MessageType { get; }
        public void Update(IWebsocketMessageBase oMessage);
    }

    /// <summary>
    /// Websocket message
    /// </summary>
    public interface IWebsocketMessage: IWebsocketMessageBase
    {
        public IFuturesSymbol Symbol { get; }

    }
}

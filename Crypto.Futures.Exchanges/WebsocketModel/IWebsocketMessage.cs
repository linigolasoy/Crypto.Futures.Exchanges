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
        Subscription
        
    }

    /// <summary>
    /// Message base
    /// </summary>
    public interface IWebsocketMessageBase
    {
        public WsMessageType MessageType { get; }
    }

    /// <summary>
    /// Websocket message
    /// </summary>
    public interface IWebsocketMessage: IWebsocketMessageBase
    {
        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage);
    }
}

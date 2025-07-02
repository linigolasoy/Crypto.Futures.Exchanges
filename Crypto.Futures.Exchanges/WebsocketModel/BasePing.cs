using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    public class BasePing : IWebsocketMessageBase
    {
        public WsMessageType MessageType { get => WsMessageType.Ping; }
    }
    public class BasePong : IWebsocketMessageBase
    {
        public WsMessageType MessageType { get => WsMessageType.Pong; }
    }
}

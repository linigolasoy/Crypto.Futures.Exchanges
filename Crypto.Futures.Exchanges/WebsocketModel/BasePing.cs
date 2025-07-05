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

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
    public class BasePong : IWebsocketMessageBase
    {
        public WsMessageType MessageType { get => WsMessageType.Pong; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}

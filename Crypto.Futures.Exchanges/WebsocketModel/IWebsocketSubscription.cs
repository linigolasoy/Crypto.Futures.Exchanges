using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    public interface IWebsocketSubscription: IWebsocketMessage
    {
        public WsMessageType SubscriptionType { get; }
    }
}

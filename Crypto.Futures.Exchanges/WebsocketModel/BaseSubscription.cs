using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    public class BaseSubscription : IWebsocketSubscription
    {
        public BaseSubscription(WsMessageType subscriptionType, IFuturesSymbol symbol)
        {
            SubscriptionType = subscriptionType;
            Symbol = symbol;
        }

        public WsMessageType SubscriptionType { get; }

        public WsMessageType MessageType { get => WsMessageType.Subscription; }

        public IFuturesSymbol Symbol { get; }
        public void Update(IWebsocketMessage oMessage)
        {
            return;
        }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}

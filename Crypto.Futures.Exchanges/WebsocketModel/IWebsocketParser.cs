using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Internal methods for websocket
    /// </summary>
    public interface IWebsocketParser
    {
        public IFuturesExchange Exchange { get; }
        public int PingSeconds { get; }
        public int MaxSubscriptions { get; }

        // public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame);
        public string? ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType);
        public string[]? ParseSubscription(IFuturesSymbol[] aSymbols, WsMessageType eSubscriptionType);
        public string ParsePing();
        public string ParsePong();

        public IWebsocketMessage[]? ParseMessage(string strMessage);
    }
}

using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Data
{
    internal class HyperTicker : ITicker
    {
        public HyperTicker(IFuturesSymbol oSymbol, DateTime dtTime, decimal dLastPrice)
        {
            Symbol = oSymbol;
            DateTime = dtTime;
            LastPrice = dLastPrice;
        }

        public DateTime DateTime { get; }

        public decimal LastPrice { get; }

        public decimal AskPrice { get; internal set; } = 0;

        public decimal BidPrice { get; internal set; } = 0;

        public decimal AskVolume { get; internal set; } = 0;

        public decimal BidVolume { get; internal set; } = 0;

        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}

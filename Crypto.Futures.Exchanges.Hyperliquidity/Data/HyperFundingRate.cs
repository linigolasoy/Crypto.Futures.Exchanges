using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Data
{
    internal class HyperFundingRate : IFundingRate
    {
        public HyperFundingRate(IFuturesSymbol oSymbol, DateTime dtNext, decimal dRate)
        {
            Symbol = oSymbol;
            Next = dtNext;
            Rate = dRate;
        }
        public DateTime Next { get; }

        public decimal Rate { get; }

        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get => WsMessageType.FundingRate; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}

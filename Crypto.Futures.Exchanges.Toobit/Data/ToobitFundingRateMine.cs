using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toobit.Net.Objects.Models;

namespace Crypto.Futures.Exchanges.Toobit.Data
{
    internal class ToobitFundingRateMine : IFundingRate
    {
        public ToobitFundingRateMine(IFuturesSymbol oSymbol, ToobitFundingRate oJson)
        {
            Symbol = oSymbol;
            Rate = oJson.FundingRate;
            Next = oJson.NextFundingTime.ToLocalTime();
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

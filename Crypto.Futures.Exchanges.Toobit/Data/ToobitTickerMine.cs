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
    internal class ToobitTickerMine : ITicker
    {
        public ToobitTickerMine( IFuturesSymbol oSymbol, ToobitTicker oJson)
        {
            Symbol = oSymbol;
            DateTime = DateTime.Now;
            LastPrice = oJson.LastPrice!.Value;
            AskPrice = LastPrice;
            BidPrice = LastPrice;
            AskVolume = oJson.Volume;
            BidVolume = oJson.Volume;
        }


        public DateTime DateTime { get; }

        public decimal LastPrice { get; }

        public decimal AskPrice { get; }

        public decimal BidPrice { get; }

        public decimal AskVolume { get; }

        public decimal BidVolume { get; }

        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            throw new NotImplementedException();
        }
    }
}

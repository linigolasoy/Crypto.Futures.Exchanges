using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using Crypto.Futures.Exchanges.WebsocketModel;
using BloFin.Net.Objects.Models;

namespace Crypto.Futures.Exchanges.Blofin.Data
{

    internal class BlofinTickerMine: ITicker
    {
        public BlofinTickerMine (IFuturesSymbol oSymbol, BloFinTicker oJson )
        {
            Symbol = oSymbol;
            DateTime = oJson.Timestamp.ToLocalTime();
            LastPrice = oJson.LastPrice;
            AskPrice = oJson.BestAskPrice;
            AskVolume = oJson.BestAskQuantity * oSymbol.ContractSize;
            BidPrice = oJson.BestBidPrice;
            BidVolume = oJson.BestBidQuantity * oSymbol.ContractSize;
        }
        public DateTime DateTime { get; private set; }

        public decimal LastPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidVolume { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        
        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is ITicker)) return;
            ITicker oTicker = (ITicker)oMessage;
            DateTime = oTicker.DateTime;
            AskPrice = oTicker.AskPrice;
            BidPrice = oTicker.BidPrice;
            AskVolume = oTicker.AskVolume;
            BidVolume = oTicker.BidVolume;
            LastPrice = oTicker.LastPrice;

        }

    }
}

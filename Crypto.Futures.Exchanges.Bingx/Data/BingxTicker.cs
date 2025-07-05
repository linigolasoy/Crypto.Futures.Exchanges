using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Crypto.Futures.Exchanges.Bingx.Data
{

    internal class BingxTicker: ITicker
    {
        public BingxTicker(IFuturesSymbol oSymbol, BingXFuturesTicker oJson )
        {
            Symbol = oSymbol;
            DateTime = DateTime.Now;
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

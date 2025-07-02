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

    internal class BingxOrderbookPrice : IOrderbookPrice
    {

        public BingxOrderbookPrice(IFuturesSymbol oSymbol, BingXBookTickerUpdate oJson)
        {
            Symbol = oSymbol;
            DateTime = oJson.EventTime.ToLocalTime();
            AskPrice = oJson.BestAskPrice;
            AskVolume = oJson.BestAskQuantity * oSymbol.ContractSize;
            BidPrice = oJson.BestBidPrice;
            BidVolume = oJson.BestBidQuantity * oSymbol.ContractSize;
        }
        public DateTime DateTime { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal BidVolume { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.OrderbookPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IOrderbookPrice)) return;
            IOrderbookPrice oPrice = (IOrderbookPrice)oMessage;
            DateTime = oPrice.DateTime;
            AskPrice = oPrice.AskPrice;
            AskVolume = oPrice.AskVolume;
            BidPrice = oPrice.BidPrice;
            BidVolume = oPrice.BidVolume;
        }

    }
}

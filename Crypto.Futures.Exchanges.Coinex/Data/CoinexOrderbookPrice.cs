using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Crypto.Futures.Exchanges.Coinex.Data
{


    internal class CoinexOrderbookPrice : IOrderbookPrice
    {

        public CoinexOrderbookPrice(IFuturesSymbol oSymbol, CoinExBookPriceUpdate oJson, DateTime? dTime )
        {
            Symbol = oSymbol;
            DateTime = (dTime == null ? DateTime.Now : dTime.Value.ToLocalTime());
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

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IOrderbookPrice)) return;
            IOrderbookPrice oOrderbookPrice = (IOrderbookPrice)oMessage;
            DateTime = oOrderbookPrice.DateTime;
            AskPrice = oOrderbookPrice.AskPrice;
            AskVolume = oOrderbookPrice.AskVolume;
            BidPrice = oOrderbookPrice.BidPrice;
            BidVolume = oOrderbookPrice.BidVolume;

        }

    }
}

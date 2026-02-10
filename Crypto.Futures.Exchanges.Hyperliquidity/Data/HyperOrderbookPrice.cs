using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Data
{
    internal class HyperOrderbookPrice : IOrderbookPrice
    {
        public HyperOrderbookPrice(IFuturesSymbol oSymbol, HyperLiquidBookTicker oTicker)
        {
            Symbol = oSymbol;
            DateTime = oTicker.Timestamp.ToLocalTime();
            AskPrice = oTicker.BestAsk.Price;
            AskVolume = oTicker.BestAsk.Quantity;
            BidPrice = oTicker.BestBid.Price;
            BidVolume = oTicker.BestBid.Quantity;
        }
        public DateTime DateTime { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal BidVolume { get; private set; }

        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get=> WsMessageType.OrderbookPrice; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IOrderbookPrice)) return;
            IOrderbookPrice oPrice = (IOrderbookPrice)oMessage;
            AskPrice = oPrice.AskPrice;
            AskVolume = oPrice.AskVolume;
            BidPrice = oPrice.BidPrice;
            BidVolume = oPrice.BidVolume;
            DateTime = oPrice.DateTime;
        }
    }
}

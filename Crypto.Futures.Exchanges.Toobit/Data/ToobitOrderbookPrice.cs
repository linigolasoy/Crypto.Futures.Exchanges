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
    /// <summary>
    /// Orderbook price implementation
    /// </summary>
    internal class ToobitOrderbookPrice : IOrderbookPrice
    {
        public ToobitOrderbookPrice(IFuturesSymbol oSymbol, ToobitOrderBookUpdate oUpdate) 
        { 
            Symbol = oSymbol;
            DateTime = oUpdate.Timestamp.ToLocalTime();
            AskPrice = oUpdate.Asks[0].Price;
            AskVolume = oUpdate.Asks[0].Quantity * oSymbol.ContractSize;
            BidPrice = oUpdate.Bids[0].Price;
            BidVolume = oUpdate.Bids[0].Quantity * oSymbol.ContractSize;
        }

        public DateTime DateTime { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal BidVolume { get; private set; }

        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get => WsMessageType.OrderbookPrice; }

        public void Update(IWebsocketMessageBase oMessage)
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

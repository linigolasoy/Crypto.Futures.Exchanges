using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    
    internal class BitgetOrderbookPrice : IOrderbookPrice
    {
        public BitgetOrderbookPrice(IFuturesSymbol oSymbol, BitgetFuturesTickerUpdate oJson)
        {
            Symbol = oSymbol;
            AskPrice = (oJson.BestAskPrice == null ? oJson.LastPrice : oJson.BestAskPrice.Value);
            AskVolume = (oJson.BestAskQuantity == null ? 0 : oJson.BestAskQuantity.Value);
            BidPrice = (oJson.BestBidPrice == null ? oJson.LastPrice : oJson.BestBidPrice.Value);
            BidVolume = (oJson.BestBidQuantity == null ? 0 : oJson.BestBidQuantity.Value);
            DateTime = oJson.Timestamp.ToLocalTime();
        }
        public BitgetOrderbookPrice(IFuturesSymbol oSymbol, BitgetOrderBookUpdate oUpdate)
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

        public WsMessageType MessageType { get => WsMessageType.OrderbookPrice; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IOrderbookPrice)) return;
            IOrderbookPrice oOrderbookPrice = (IOrderbookPrice)oMessage;
            AskPrice = oOrderbookPrice.AskPrice;
            AskVolume = oOrderbookPrice.AskVolume;
            BidPrice = oOrderbookPrice.BidPrice;
            BidVolume = oOrderbookPrice.BidVolume;
            DateTime = oOrderbookPrice.DateTime;
        }
    }
}

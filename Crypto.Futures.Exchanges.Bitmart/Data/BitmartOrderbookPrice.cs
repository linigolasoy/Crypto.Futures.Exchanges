using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartOrderbookPrice: IOrderbookPrice
    {

        public BitmartOrderbookPrice(IFuturesSymbol oSymbol, BitMartBookTicker oTicker, DateTime dDate )
        {
            Symbol = oSymbol;
            DateTime = dDate.ToLocalTime();
            AskPrice = oTicker.BestBidPrice;
            AskVolume = oTicker.BestAskQuantity;
            BidPrice = oTicker.BestBidPrice;
            BidVolume = oTicker.BestBidQuantity;
        }
        public DateTime DateTime { get; private set; }

        public decimal AskPrice { get; private set; } = -1;

        public decimal AskVolume { get; private set; } = -1;

        public decimal BidPrice { get; private set; } = -1;

        public decimal BidVolume { get; private set; } = -1;

        public WsMessageType MessageType { get => WsMessageType.OrderbookPrice; }

        public IFuturesSymbol Symbol { get; }


        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IOrderbookPrice)) return;
            IOrderbookPrice oOrderbookPrice = (IOrderbookPrice)oMessage;
            DateTime = oOrderbookPrice.DateTime;
            AskPrice = (oOrderbookPrice.AskPrice < 0 ? AskPrice : oOrderbookPrice.AskPrice);
            AskVolume = (oOrderbookPrice.AskVolume < 0 ? AskVolume : oOrderbookPrice.AskVolume);
            BidPrice = (oOrderbookPrice.BidPrice < 0 ? BidPrice : oOrderbookPrice.BidPrice);
            BidVolume = (oOrderbookPrice.BidVolume < 0 ? BidVolume : oOrderbookPrice.BidVolume);
        }
    }
}

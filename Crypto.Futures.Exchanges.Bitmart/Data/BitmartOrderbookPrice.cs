using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartOrderbookPrice: IOrderbookPrice
    {

        public BitmartOrderbookPrice(IFuturesSymbol oSymbol, BitMartFuturesFullOrderBookUpdate oBook )
        {
            Symbol = oSymbol;
            DateTime = oBook.Timestamp.ToLocalTime();
            AskPrice = oBook.Asks[0].Price;
            AskVolume = oBook.Asks[0].Quantity;
            BidPrice = oBook.Bids[0].Price;
            BidVolume = oBook.Bids[0].Quantity;
        }
        public DateTime DateTime { get; private set; }

        public decimal AskPrice { get; private set; } = -1;

        public decimal AskVolume { get; private set; } = -1;

        public decimal BidPrice { get; private set; } = -1;

        public decimal BidVolume { get; private set; } = -1;

        public WsMessageType MessageType { get => WsMessageType.OrderbookPrice; }

        public IFuturesSymbol Symbol { get; }


        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IOrderbookPrice)) return;
            IOrderbookPrice oOrderbookPrice = (IOrderbookPrice)oMessage;
            if( oOrderbookPrice.AskPrice <= 0 || oOrderbookPrice.BidPrice <= 0 ) return;        
            DateTime = oOrderbookPrice.DateTime;
            AskPrice = oOrderbookPrice.AskPrice;
            AskVolume = (oOrderbookPrice.AskVolume < 0 ? AskVolume : oOrderbookPrice.AskVolume);
            BidPrice = oOrderbookPrice.BidPrice;
            BidVolume = (oOrderbookPrice.BidVolume < 0 ? BidVolume : oOrderbookPrice.BidVolume);
        }
    }
}

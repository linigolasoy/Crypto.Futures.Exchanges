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
        public BitgetOrderbookPrice(IFuturesSymbol oSymbol, BitgetTickerJson oJson)
        {
            Symbol = oSymbol;
            AskPrice = decimal.Parse(oJson.AskPrice, System.Globalization.CultureInfo.InvariantCulture);
            AskVolume = decimal.Parse(oJson.AskSize, System.Globalization.CultureInfo.InvariantCulture) * oSymbol.ContractSize;
            BidPrice = decimal.Parse(oJson.BidPrice, System.Globalization.CultureInfo.InvariantCulture);
            BidVolume = decimal.Parse(oJson.BidSize, System.Globalization.CultureInfo.InvariantCulture) * oSymbol.ContractSize;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp, true);
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
            IOrderbookPrice oOrderbookPrice = (IOrderbookPrice)oMessage;
            AskPrice = oOrderbookPrice.AskPrice;
            AskVolume = oOrderbookPrice.AskVolume;
            BidPrice = oOrderbookPrice.BidPrice;
            BidVolume = oOrderbookPrice.BidVolume;
            DateTime = oOrderbookPrice.DateTime;
        }
    }
}

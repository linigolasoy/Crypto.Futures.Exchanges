using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartOrderbookPriceJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("best_bid_price")]
        public string BestBidPrice { get; set; } = string.Empty; // Best bid price
        [JsonProperty("best_bid_vol")]
        public string BestBidVol { get; set; } = string.Empty; // Best bid volume
        [JsonProperty("best_ask_price")]
        public string BestAskPrice { get; set; } = string.Empty; // Best ask price
        [JsonProperty("best_ask_vol")]
        public string BestAskVol { get; set; } = string.Empty; // Best ask volume
        [JsonProperty("ms_t")]
        public long Timestamp { get; set; } // Timestamp in milliseconds
    }
    internal class BitmartOrderbookPrice: IOrderbookPrice
    {
        public BitmartOrderbookPrice(IFuturesSymbol oSymbol, BitmartOrderbookPriceJson oJson)
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp,true);
            AskPrice = decimal.Parse(oJson.BestAskPrice, System.Globalization.CultureInfo.InvariantCulture);
            AskVolume = decimal.Parse(oJson.BestAskVol, System.Globalization.CultureInfo.InvariantCulture);
            BidPrice = decimal.Parse(oJson.BestBidPrice, System.Globalization.CultureInfo.InvariantCulture);
            BidVolume = decimal.Parse(oJson.BestBidVol, System.Globalization.CultureInfo.InvariantCulture);
        }
        public DateTime DateTime { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal BidVolume { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.OrderbookPrice; }

        public IFuturesSymbol Symbol { get; }

        public static IWebsocketMessage[]? ParseWs( IFuturesSymbol oSymbol, JToken? oToken )
        {
            if (oToken == null) return null;
            BitmartOrderbookPriceJson? oJson = oToken.ToObject<BitmartOrderbookPriceJson>();
            if (oJson == null) return null;
            if (oJson.Symbol != oSymbol.Symbol) return null;
            return new IWebsocketMessage[] { new BitmartOrderbookPrice(oSymbol, oJson) };
        }

        public void Update(IWebsocketMessage oMessage)
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

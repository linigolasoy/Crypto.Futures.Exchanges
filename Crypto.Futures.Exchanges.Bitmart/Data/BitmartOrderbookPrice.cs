using BitMart.Net.Objects.Models;
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
            decimal nValue = -1;
            if(decimal.TryParse(oJson.BestAskPrice, System.Globalization.CultureInfo.InvariantCulture, out nValue))
            {
                AskPrice = nValue;
            }
            if (decimal.TryParse(oJson.BestAskVol, System.Globalization.CultureInfo.InvariantCulture, out nValue))
            {
                AskVolume = nValue;
            }
            if (decimal.TryParse(oJson.BestBidPrice, System.Globalization.CultureInfo.InvariantCulture, out nValue))
            {
                BidPrice = nValue;
            }
            if (decimal.TryParse(oJson.BestBidVol, System.Globalization.CultureInfo.InvariantCulture, out nValue))
            {
                BidVolume = nValue;
            }
        }

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
            AskPrice = (oOrderbookPrice.AskPrice < 0 ? AskPrice : oOrderbookPrice.AskPrice);
            AskVolume = (oOrderbookPrice.AskVolume < 0 ? AskVolume : oOrderbookPrice.AskVolume);
            BidPrice = (oOrderbookPrice.BidPrice < 0 ? BidPrice : oOrderbookPrice.BidPrice);
            BidVolume = (oOrderbookPrice.BidVolume < 0 ? BidVolume : oOrderbookPrice.BidVolume);
        }
    }
}

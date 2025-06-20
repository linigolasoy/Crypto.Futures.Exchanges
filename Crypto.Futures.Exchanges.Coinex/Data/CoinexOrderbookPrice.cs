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

    internal class CoinexBboJson
    {
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; } = 0;
        [JsonProperty("best_bid_price")]
        public string BestBidPrice { get; set; } = string.Empty;
        [JsonProperty("best_bid_size")]
        public string BestBidSize { get; set; } = string.Empty;
        [JsonProperty("best_ask_price")]
        public string BestAskPrice { get; set; } = string.Empty;
        [JsonProperty("best_ask_size")]
        public string BestAskSize { get; set; } = string.Empty;
    }
    internal class CoinexOrderbookPrice : IOrderbookPrice
    {

        public CoinexOrderbookPrice(IFuturesSymbol oSymbol, CoinexBboJson oJson )
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.UpdatedAt, true);
            AskPrice = decimal.Parse(oJson.BestAskPrice, CultureInfo.InvariantCulture);
            AskVolume = decimal.Parse(oJson.BestAskSize, CultureInfo.InvariantCulture);
            BidPrice = decimal.Parse(oJson.BestBidPrice, CultureInfo.InvariantCulture);
            BidVolume = decimal.Parse(oJson.BestBidSize, CultureInfo.InvariantCulture);
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
            DateTime = oOrderbookPrice.DateTime;
            AskPrice = oOrderbookPrice.AskPrice;
            AskVolume = oOrderbookPrice.AskVolume;
            BidPrice = oOrderbookPrice.BidPrice;
            BidVolume = oOrderbookPrice.BidVolume;

        }

        public static IWebsocketMessage[]? ParseWs( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            CoinexBboJson? oBbo = oToken.ToObject<CoinexBboJson>();
            if (oBbo == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oBbo.Market);
            if (oSymbol == null) return null;
            return new IWebsocketMessage[]
            {
                new CoinexOrderbookPrice(oSymbol, oBbo)
            };

        }
    }
}

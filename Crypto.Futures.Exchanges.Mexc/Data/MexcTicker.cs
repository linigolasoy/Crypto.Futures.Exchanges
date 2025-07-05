using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;
using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using Crypto.Futures.Exchanges.WebsocketModel;

namespace Crypto.Futures.Exchanges.Mexc.Data
{
    internal class MexcTickerJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("lastPrice")]
        public decimal LastPrice { get; set; } = 0;
        [JsonProperty("bid1")]
        public decimal Bid1 { get; set; } = 0;
        [JsonProperty("ask1")]
        public decimal Ask1 { get; set; } = 0;
        [JsonProperty("volume24")]
        public decimal Volume24 { get; set; } = 0;
        [JsonProperty("amount24")]
        public decimal Amount24 { get; set; } = 0;
        [JsonProperty("holdVol")]
        public decimal HoldVol { get; set; } = 0;
        [JsonProperty("lower24Price")]
        public decimal Lower24Price { get; set; } = 0;
        [JsonProperty("high24Price")]
        public decimal High24Price { get; set; } = 0;
        [JsonProperty("riseFallRate")]
        public decimal RiseFallRate { get; set; } = 0;
        [JsonProperty("riseFallValue")]
        public decimal RiseFallValue { get; set; } = 0;
        [JsonProperty("indexPrice")]
        public decimal IndexPrice { get; set; } = 0;
        [JsonProperty("fairPrice")]
        public decimal FairPrice { get; set; } = 0;
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; } = 0;
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; } = 0;
    }


    internal class MexcTicker: ITicker
    {
        public MexcTicker(IFuturesSymbol oSymbol, MexcTickerJson oJson )
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp, true);
            AskPrice = oJson.Ask1;
            BidPrice = oJson.Bid1;
            AskVolume = 0;
            BidVolume = 0;
            LastPrice = oJson.LastPrice;
        }
        public DateTime DateTime { get; private set; }

        public decimal LastPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidVolume { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if( !(oMessage is ITicker) ) return;
            ITicker oTicker = (ITicker)oMessage;
            DateTime = oTicker.DateTime;
            AskPrice = oTicker.AskPrice;
            BidPrice = oTicker.BidPrice;
            AskVolume = oTicker.AskVolume;
            BidVolume = oTicker.BidVolume;
            LastPrice = oTicker.LastPrice;

        }

        public static ITicker? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            MexcTickerJson? oJson = oToken.ToObject<MexcTickerJson>();  
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;

            return new MexcTicker(oSymbol, oJson);
        }

        public static IWebsocketMessage[]? ParseWs( IFuturesExchange oExchange, JToken? oToken )
        {
            ITicker? oTicker = Parse(oExchange, oToken);
            if (oTicker == null) return null;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();
            IOrderbookPrice oPrice = new MexcOrderbookPrice(oTicker);
            if( oPrice.AskPrice > 0 && oPrice.BidPrice > 0 )
            {
                aResult.Add(oPrice);
            }
            ILastPrice oLast = new MexcLastPrice(oTicker);
            if( oLast.Price > 0 )
            {
                aResult.Add(oLast);
            }
            return aResult.ToArray();
        }
    }
}

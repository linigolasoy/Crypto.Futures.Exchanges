using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartTradeJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("deal_price")]
        public string Price { get; set; } = string.Empty;
        [JsonProperty("trade_id")]
        public long Id { get; set; } = 0;
        [JsonProperty("deal_vol")]
        public string Volume { get; set; } = string.Empty;
        /*
        -1=buy_open_long sell_open_short
        -2=buy_open_long sell_close_long
        -3=buy_close_short sell_open_short
        -4=buy_close_short sell_close_long
        -5=sell_open_short buy_open_long
        -6=sell_open_short buy_close_short
        -7=sell_close_long buy_open_long
        -8=sell_close_long buy_close_short
        */
        [JsonProperty("way")]
        public int Way { get; set; } = 0;
        [JsonProperty("m")] // -true=buyer is maker -false=seller is maker

        public bool MakerTaker { get; set; } = false;
        [JsonProperty("created_at")]
        public string Timestamp { get; set; } = string.Empty;
    }
    internal class BitmartTrade: ITrade
    {
        public BitmartTrade( IFuturesSymbol oSymbol, BitmartTradeJson oJson )
        {
            Symbol = oSymbol;
            DateTime = DateTime.Parse(oJson.Timestamp, CultureInfo.InvariantCulture);
            IsBuy = (oJson.Way < 5);
            Price = decimal.Parse(oJson.Price, CultureInfo.InvariantCulture);
            Volume = decimal.Parse(oJson.Volume, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
        }
        public DateTime DateTime { get; private set; }

        public decimal Price { get; private set; }

        public decimal Volume { get; private set; }

        public bool IsBuy { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Trade; }

        public IFuturesSymbol Symbol { get; }

        public static IWebsocketMessage[]? Parse( IFuturesSymbol oSymbol, JToken oToken )
        {
            if( !(oToken is JArray)) return null;   
            JArray oArray = (JArray)oToken;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            foreach (var oItem in oArray)
            {
                BitmartTradeJson? oJson = oItem.ToObject<BitmartTradeJson>();
                if (oJson == null) continue;
                aResult.Add(new BitmartTrade(oSymbol, oJson));
            }
            return aResult.ToArray();
        }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is ITrade)) return;
            ITrade oTrade = (ITrade)oMessage;
            if (oTrade.DateTime < this.DateTime) return;
            DateTime = oTrade.DateTime;
            IsBuy = oTrade.IsBuy;
            Price = oTrade.Price;
            Volume = oTrade.Volume;

        }
    }
}

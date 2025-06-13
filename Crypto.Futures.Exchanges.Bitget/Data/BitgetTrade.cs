using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{

    internal class BitgetTradeJson
    {
        [JsonProperty("ts")]
        public string Timestamp { get; set; } = string.Empty;
        [JsonProperty("price")]
        public string Price { get; set; } = string.Empty;
        [JsonProperty("size")]
        public string Volume { get; set; } = string.Empty;
        [JsonProperty("side")] // sell/buy
        public string Side { get; set; } = string.Empty;
        [JsonProperty("tradeId")] 
        public string Id { get; set; } = string.Empty;
    }
    internal class BitgetTrade: ITrade
    {
        public BitgetTrade( IFuturesSymbol oSymbol, BitgetTradeJson oJson )
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp, true);
            Price = decimal.Parse(oJson.Price, CultureInfo.InvariantCulture);
            Volume = decimal.Parse(oJson.Volume, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
            IsBuy = (oJson.Side == "buy");
        }
        public DateTime DateTime { get; private set; }

        public decimal Price { get; private set; }

        public decimal Volume { get; private set; }

        public bool IsBuy { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Trade; }

        public IFuturesSymbol Symbol { get; }

        public static IWebsocketMessage[]? Parse( IFuturesSymbol oSymbol, JToken oToken )
        {
            if( !(oToken is JArray )) return null;  
            JArray aArray = ( JArray )oToken;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            foreach (var oItem in aArray)
            {
                BitgetTradeJson? oJson = oItem.ToObject<BitgetTradeJson>();
                if (oJson == null) continue;
                aResult.Add(new BitgetTrade(oSymbol, oJson));
            }
            return aResult.ToArray();
        }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is ITrade)) return;
            ITrade oTrade = (ITrade)oMessage;
            if (oTrade.DateTime < DateTime) return;
            DateTime = oTrade.DateTime;
            Price = oTrade.Price;
            Volume = oTrade.Volume;
            IsBuy = oTrade.IsBuy;
        }
    }
}

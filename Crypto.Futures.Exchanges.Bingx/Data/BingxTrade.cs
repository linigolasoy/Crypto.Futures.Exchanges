using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Data
{

    internal class BingxTradeWs
    {
        [JsonProperty("T")]
        public long Timestamp { get; set; } = 0;

        [JsonProperty("s")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("m")]
        public bool IsSell { get; set; } = false;
        [JsonProperty("p")]
        public string Price { get; set; } = string.Empty;
        [JsonProperty("q")]
        public string Volume { get; set; } = string.Empty;
    }

    internal class BingxTrade: ITrade
    {
        public BingxTrade( IFuturesSymbol oSymbol, BingxTradeWs oJson )
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp, true);
            Price = decimal.Parse(oJson.Price, CultureInfo.InvariantCulture);
            Volume = decimal.Parse(oJson.Volume, CultureInfo.InvariantCulture);
            IsBuy = !oJson.IsSell;
        }
        public DateTime DateTime { get; private set; }

        public decimal Price { get; private set; }

        public decimal Volume { get; private set; }

        public bool IsBuy { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Trade; }

        public IFuturesSymbol Symbol { get; }

        public static IWebsocketMessage[]? Parse( IFuturesExchange oExchange, string strSymbol, JToken? oData )
        {
            if( oData == null ) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol( strSymbol );
            if( oSymbol == null ) return null;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            if( oData is JArray )
            {
                JArray oArray = ( JArray )oData;
                foreach( var oItem in oArray )
                {
                    BingxTradeWs? oWs = oItem.ToObject<BingxTradeWs>();
                    if( oWs ==  null ) continue;   
                    aResult.Add(new BingxTrade(oSymbol, oWs));
                }
            }
            else
            {
                BingxTradeWs? oWs = oData.ToObject<BingxTradeWs>();
                if (oWs == null) return null;
                aResult.Add(new BingxTrade(oSymbol, oWs));
            }

            return aResult.ToArray();
        }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is ITrade) ) return;
            ITrade oTrade = (ITrade)oMessage;   
            if( oTrade.DateTime < this.DateTime ) return;
            DateTime oDateTime = oTrade.DateTime;
            Price = oTrade.Price;
            Volume = oTrade.Volume;
            IsBuy = oTrade.IsBuy;
        }
    }
}

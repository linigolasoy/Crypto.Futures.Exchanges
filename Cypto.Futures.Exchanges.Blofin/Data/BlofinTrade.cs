using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Data
{


    internal class BlofinTradeWsJson
    {
        [JsonProperty("instId")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("tradeId")]
        public string TradeId { get; set; } = string.Empty;
        [JsonProperty("price")]
        public string Price { get; set; } = string.Empty;
        [JsonProperty("size")]
        public string Size { get; set; } = string.Empty;
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;
        [JsonProperty("ts")]
        public string Timestamp { get; set; } = string.Empty;
    }

    /// <summary>
    /// Trade parsed object
    /// </summary>
    internal class BlofinTrade : ITrade
    {
        public BlofinTrade( IFuturesSymbol oSymbol, BlofinTradeWsJson oJson)
        {
            Symbol = oSymbol;
            DateTime = Util.FromUnixTimestamp(oJson.Timestamp, true);   
            Price = decimal.Parse(oJson.Price, CultureInfo.InvariantCulture);
            Volume = decimal.Parse(oJson.Size, CultureInfo.InvariantCulture) * oSymbol.ContractSize;
            IsBuy = (oJson.Side == "buy");
        }
        public WsMessageType MessageType { get => WsMessageType.Trade; }

        public IFuturesSymbol Symbol { get; }

        public DateTime DateTime { get; private set; }

        public decimal Price { get; private set; }

        public decimal Volume { get; private set; }

        public bool IsBuy { get; private set; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is ITrade)) return;
            ITrade oTrade = (ITrade)oMessage;   
            DateTime = oTrade.DateTime;
            Price = oTrade.Price;
            Volume = oTrade.Volume;
            IsBuy = oTrade.IsBuy;
        }

        public static IWebsocketMessage[]? ParseWs( IFuturesExchange oExchange, JToken? oData )
        {
            if( oData == null ) return null;    
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            if( oData is JArray )
            {
                JArray aArray = ( JArray )oData; 
                foreach (JToken oItem in aArray)
                {
                    BlofinTradeWsJson? oParsed = oItem.ToObject<BlofinTradeWsJson>();
                    if (oParsed == null) return null;
                    IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol( oParsed.Symbol );  
                    if (oSymbol == null) continue;
                    aResult.Add(new BlofinTrade(oSymbol, oParsed));
                }
            }
            else
            {
                BlofinTradeWsJson? oParsed = oData.ToObject<BlofinTradeWsJson>();
                if (oParsed == null) return null;
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oParsed.Symbol);
                if (oSymbol == null) return null;
                aResult.Add(new BlofinTrade(oSymbol, oParsed));
            }
            return aResult.ToArray();   
        }
    }
}

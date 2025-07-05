using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{

    internal class MexcTradeWs
    {
        [JsonProperty("p")]
        public decimal Price { get; set; } = 0;
        [JsonProperty("v")]
        public decimal Volume { get; set; } = 0;
        [JsonProperty("T")]
        public int Direction { get; set; } = 0;
        [JsonProperty("O")]
        public int OpenMode { get; set; } = 0;
        [JsonProperty("M")]
        public int Auto { get; set; } = 0;
        [JsonProperty("t")]
        public long Timestamp { get; set; } = 0;
    }
    internal class MexcTrade : ITrade
    {

        internal MexcTrade(IFuturesSymbol oSymbol, MexcTradeWs oWs)
        {
            DateTime = Util.FromUnixTimestamp(oWs.Timestamp,true);
            Symbol = oSymbol;
            Price = oWs.Price;
            Volume = oWs.Volume * oSymbol.ContractSize;
            IsBuy = (oWs.Direction == 1);
        }
        public DateTime DateTime { get; private set; }

        public decimal Price { get; private set; }

        public decimal Volume { get; private set; }

        public bool IsBuy { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Trade; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if( !(oMessage is ITrade)) return;
            ITrade oTrade = (ITrade)oMessage;
            DateTime = oTrade.DateTime;
            Price = oTrade.Price;
            Volume = oTrade.Volume;
            IsBuy=oTrade.IsBuy;
        }

        public static IWebsocketMessage[]? ParseWs(IFuturesExchange oExchange, string? strSymbol, JToken? oData )
        {
            if (strSymbol == null || oData == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(strSymbol); 
            if (oSymbol == null) return null;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();
            if( oData is JArray )
            {
                foreach( JToken oData2 in oData )
                {
                    MexcTradeWs? oWs = oData2.ToObject<MexcTradeWs>();
                    if (oWs == null) continue;
                    aResult.Add(new MexcTrade(oSymbol, oWs));
                }
            }
            else
            {
                MexcTradeWs? oWs = oData.ToObject<MexcTradeWs>();
                if (oWs == null) return null;
                aResult.Add( new MexcTrade(oSymbol, oWs));
            }
            return aResult.ToArray();
        }
    }
}

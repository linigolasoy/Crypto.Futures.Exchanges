using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Data
{
    internal class CoinexTradeWsDeal
    {
        [JsonProperty("deal_id")]
        public long DealId { get; set; } = 0;
        [JsonProperty("created_at")]
        public long Timestamp { get; set; } = 0;
        [JsonProperty("side")]
        public string Side { get; set; } = string.Empty;
        [JsonProperty("price")]
        public string Price { get; set; } = string.Empty;
        [JsonProperty("amount")]
        public string Volume { get; set; } = string.Empty;
    }

    internal class CoinexTradeWsJson
    {
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        [JsonProperty("deal_list")]
        public List<CoinexTradeWsDeal> Deals { get; set; } = new List<CoinexTradeWsDeal>();
    }
    internal class CoinexTrade : ITrade
    {

        public CoinexTrade( IFuturesSymbol oSymbol, CoinexTradeWsDeal oJson)
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

        public static IWebsocketMessage[]? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if( oToken == null ) return null;
            CoinexTradeWsJson? oJson = oToken.ToObject<CoinexTradeWsJson>();    
            if( oJson == null ) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Market);  
            if( oSymbol == null ) return null;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();
            foreach (var oTrade in oJson.Deals)
            {
                aResult.Add(new CoinexTrade(oSymbol, oTrade));    
            }

            return aResult.ToArray();
        }
    }
}

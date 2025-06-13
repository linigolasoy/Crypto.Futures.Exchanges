using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Crypto.Futures.Exchanges.WebsocketModel;

namespace Crypto.Futures.Exchanges.Coinex.Data
{

    internal class CoinexStateJson
    {
        [JsonProperty("market")]
        public string Market { get; set; } = string.Empty;
        [JsonProperty("last")]
        public string LastPrice { get; set; } = string.Empty;
        [JsonProperty("open")]
        public string Open { get; set; } = string.Empty;
        [JsonProperty("close")]
        public string Close { get; set; } = string.Empty;
        [JsonProperty("high")]
        public string High { get; set; } = string.Empty;
        [JsonProperty("low")]
        public string Low { get; set; } = string.Empty;
        [JsonProperty("volume")]
        public string Volume { get; set; } = string.Empty;
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
        [JsonProperty("volume_sell")]
        public string Volume_sell { get; set; } = string.Empty;
        [JsonProperty("volume_buy")]
        public string Volume_buy { get; set; } = string.Empty;
        [JsonProperty("open_interest_size")]
        public string Open_interest_size { get; set; } = string.Empty;
        [JsonProperty("insurance_fund_size")]
        public string Insurance_fund_size { get; set; } = string.Empty;
        [JsonProperty("latest_funding_rate")]
        public string LastFundingRate { get; set; } = string.Empty;
        [JsonProperty("next_funding_rate")]
        public string NextFundingRate { get; set; } = string.Empty;
        [JsonProperty("latest_funding_time")]
        public long LastFundingTime { get; set; } = 0;
        [JsonProperty("next_funding_time")]
        public long NextFundingTime { get; set; } = 0;
    }

    internal class CoinexFundingRateJson
    {
        [JsonProperty("market")]
        public string Market { get; set; } =string.Empty;
        [JsonProperty("mark_price")]
        public string MarkPrice { get; set; } = string.Empty;
        [JsonProperty("latest_funding_rate")]
        public string LatestFundingRate { get; set; } = string.Empty;
        [JsonProperty("next_funding_rate")]
        public string NextFundingRate { get; set; } = string.Empty;
        [JsonProperty("max_funding_rate")]
        public string MaxFundingRate { get; set; } = string.Empty;
        [JsonProperty("min_funding_rate")]
        public string MinFundingRate { get; set; } = string.Empty;
        [JsonProperty("latest_funding_time")]
        public long LatestFundingTime { get; set; } = 0;
        [JsonProperty("next_funding_time")]
        public long NextFundingTime { get; set; } = 0;
    }

    internal class CoinexFundingRate : IFundingRate
    {

        public CoinexFundingRate( IFuturesSymbol oSymbol, CoinexFundingRateJson oJson ) 
        { 
            Symbol = oSymbol;
            Next = Util.FromUnixTimestamp(oJson.LatestFundingTime, true);
            Rate = decimal.Parse(oJson.LatestFundingRate, CultureInfo.InvariantCulture);

        }

        public CoinexFundingRate(IFuturesSymbol oSymbol, CoinexStateJson oJson )
        {
            Symbol =oSymbol;
            Next = Util.FromUnixTimestamp(oJson.LastFundingTime, true);
            Rate = decimal.Parse(oJson.LastFundingRate, CultureInfo.InvariantCulture);

        }
        public IFuturesSymbol Symbol { get; }
        public WsMessageType MessageType { get => WsMessageType.FundingRate; }

        public DateTime Next { get; private set; }

        public decimal Rate { get; private set; }
        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IFundingRate)) return;
            IFundingRate oFunding = (IFundingRate)oMessage;
            Next = oFunding.Next;
            Rate = oFunding.Rate;
        }

        public static IFundingRate? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            var oFundingJson = oToken.ToObject<CoinexFundingRateJson>();
            if (oFundingJson == null) return null;
            var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Market);
            if (oSymbol == null) return null;
            return new CoinexFundingRate(oSymbol, oFundingJson);
        }

        public static IWebsocketMessage[]? ParseWs( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            if( !(oToken is JObject)) return null;
            JObject oObject = (JObject)oToken;  
            if( !oObject.ContainsKey("state_list")) return null;
            JToken? oValue = oObject["state_list"];
            if(oValue == null) return null; 
            if( !(oValue is JArray)) return null;
            JArray oArray = (JArray)oValue;

            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            foreach( var oItem in oArray )
            {
                CoinexStateJson? oJson = oItem.ToObject<CoinexStateJson>();
                if (oJson == null) continue;
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Market);  
                if (oSymbol == null) continue;
                aResult.Add( new CoinexFundingRate(oSymbol,oJson) );    
            }

            return aResult.ToArray();
        }
    }
}

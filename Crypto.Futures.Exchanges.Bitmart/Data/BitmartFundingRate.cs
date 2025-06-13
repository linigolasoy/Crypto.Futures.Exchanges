using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartFundingRateJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public string FundingRate { get; set; } = string.Empty;
        [JsonProperty("fundingTime")]
        public string FundingTime { get; set; } = string.Empty;
        [JsonProperty("nextFundingRate")]
        public string NextFundingRate { get; set; } = string.Empty;
        [JsonProperty("nextFundingTime")]
        public string NextFundingTime { get; set; } = string.Empty;
        [JsonProperty("funding_upper_limit")]
        public string Funding_upper_limit { get; set; } = string.Empty;
        [JsonProperty("funding_lower_limit")]
        public string Funding_lower_limit { get; set; } = string.Empty;
        [JsonProperty("ts")]
        public string Timestamp { get; set; } = string.Empty;
    }
    internal class BitmartFundingRate : IFundingRate
    {

        public BitmartFundingRate(IFuturesSymbol oSymbol, BitmartFundingRateJson oJson)
        {
            Symbol = oSymbol;
            Next = Util.FromUnixTimestamp(oJson.NextFundingTime, true);
            Rate = decimal.Parse(oJson.FundingRate, CultureInfo.InvariantCulture);
        }

        public BitmartFundingRate( IFuturesSymbol oSymbol, BitmartSymbolJson oJson) 
        { 
            Symbol = oSymbol;
            Next = Util.NextFundingRate(oJson.FundingIntervalHours);
            Rate = decimal.Parse(oJson.FundingRate, CultureInfo.InvariantCulture);  
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
            if (!(oToken is JObject)) return null;

            BitmartSymbolJson? oJson = oToken.ToObject<BitmartSymbolJson>();
            if (oJson == null) return null;

            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;

            return new BitmartFundingRate(oSymbol, oJson);
        }

        public static IFundingRate? Parse(IFuturesSymbol oSymbol, JToken oToken)
        {
            BitmartFundingRateJson? oJson = oToken.ToObject<BitmartFundingRateJson>();
            if( oJson == null ) return null;
            return new BitmartFundingRate(oSymbol, oJson);
        }

    }
}

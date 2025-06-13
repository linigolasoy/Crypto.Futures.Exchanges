using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Crypto.Futures.Exchanges.Bitget
{


    internal class BitgetFundingRateJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public string FundingRate { get; set; } = string.Empty;
        [JsonProperty("fundingRateInterval")]
        public string FundingRateInterval { get; set; } = string.Empty;
        [JsonProperty("nextUpdate")]
        public string NextUpdate { get; set; } = string.Empty;
        [JsonProperty("minFundingRate")]
        public string MinFundingRate { get; set; } = string.Empty;
        [JsonProperty("maxFundingRate")]
        public string MaxFundingRate { get; set; } = string.Empty;
    }
    internal class BitgetFundingRate: IFundingRate
    {
        public BitgetFundingRate(IFuturesSymbol oSymbol, BitgetFundingRateJson oJson) 
        {
            Symbol = oSymbol;
            Next = Util.FromUnixTimestamp(oJson.NextUpdate,true);
            Rate = decimal.Parse(oJson.FundingRate, CultureInfo.InvariantCulture);
        }

        public BitgetFundingRate(IFuturesSymbol oSymbol, BitgetTickerJson oJson)
        {
            Symbol = oSymbol;
            Next = Util.FromUnixTimestamp(oJson.NexFundingTime, true);
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
            var oFundingJson = oToken.ToObject<BitgetFundingRateJson>();
            if (oFundingJson == null) return null;
            var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Symbol);
            if (oSymbol == null) return null;
            return new BitgetFundingRate(oSymbol, oFundingJson);
        }

        public static IWebsocketMessage[]? Parse( IFuturesSymbol oSymbol, JToken oToken )
        {
            if( !( oToken is JArray)) return null;
            JArray oArray = (JArray)oToken;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            foreach( var oItem in oArray )
            {
                BitgetTickerJson? oJson = oItem.ToObject<BitgetTickerJson>();
                if (oJson == null) continue;
                aResult.Add(new BitgetFundingRate(oSymbol, oJson));
            }
            return aResult.ToArray();
        }
    }
}

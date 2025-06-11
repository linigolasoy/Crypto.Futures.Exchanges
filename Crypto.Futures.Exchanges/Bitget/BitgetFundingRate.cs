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
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(oJson.NextUpdate));
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            Next = dDate;
            Rate = decimal.Parse(oJson.FundingRate, CultureInfo.InvariantCulture);
        }
        public IFuturesSymbol Symbol { get; }
        public WsMessageType MessageType { get => WsMessageType.FundingRate; }  
        public DateTime Next { get; }

        public decimal Rate { get; }

        public static IFundingRate? Parse( IFuturesExchange oExchange, JToken? oToken )
        {
            if (oToken == null) return null;
            var oFundingJson = oToken.ToObject<BitgetFundingRateJson>();
            if (oFundingJson == null) return null;
            var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Symbol);
            if (oSymbol == null) return null;
            return new BitgetFundingRate(oSymbol, oFundingJson);
        }
    }
}

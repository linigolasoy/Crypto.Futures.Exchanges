using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Crypto.Futures.Exchanges;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cypto.Futures.Exchanges.Blofin.Data
{
    internal class BlofinFundingRateJson
    {
        [JsonProperty("instId")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public string Rate { get; set; } = string.Empty;
        [JsonProperty("fundingTime")]
        public string SettleTime { get; set; } = string.Empty;
    }
    internal class BlofinFundingRate : IFundingRate
    {

        public BlofinFundingRate(IFuturesSymbol oSymbol, BlofinFundingRateJson oJson)
        {
            Symbol = oSymbol;
            Next = Util.FromUnixTimestamp(oJson.SettleTime, true);
            Rate = decimal.Parse(oJson.Rate, CultureInfo.InvariantCulture);

        }
        public WsMessageType MessageType { get => WsMessageType.FundingRate; }
        public IFuturesSymbol Symbol { get; }

        public DateTime Next { get; }

        public decimal Rate { get; }

        public static IFundingRate? Parse(IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            var oFundingJson = oToken.ToObject<BlofinFundingRateJson>();
            if (oFundingJson == null) return null;
            var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Symbol);
            if (oSymbol == null) return null;
            return new BlofinFundingRate(oSymbol, oFundingJson);
        }

        public static IWebsocketMessage[]? ParseWs(IFuturesExchange oExchange, JToken? oData)
        {

            if (oData == null) return null;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();
            if (oData is JArray)
            {
                JArray aArray = (JArray)oData;
                foreach (JToken oItem in aArray)
                {
                    BlofinFundingRateJson? oParsed = oItem.ToObject<BlofinFundingRateJson>();
                    if (oParsed == null) return null;
                    IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oParsed.Symbol);
                    if (oSymbol == null) continue;
                    aResult.Add(new BlofinFundingRate(oSymbol, oParsed));
                }
            }
            else
            {
                BlofinFundingRateJson? oParsed = oData.ToObject<BlofinFundingRateJson>();
                if (oParsed == null) return null;
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oParsed.Symbol);
                if (oSymbol == null) return null;
                aResult.Add(new BlofinFundingRate(oSymbol, oParsed));
            }
            return aResult.ToArray();

        }
    }
}

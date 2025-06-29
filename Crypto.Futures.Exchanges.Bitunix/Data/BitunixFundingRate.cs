using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitunix.Data
{

    internal class BitunixFundingRateJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("markPrice")]
        public decimal MarkPrice { get; set; }
        [JsonProperty("lastPrice")]
        public decimal LastPrice { get; set; }
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; }
    }
    internal class BitunixFundingRate : IFundingRate
    {
        public BitunixFundingRate(IFuturesSymbol oSymbol, BitunixFundingRateJson oJson)
        {
            Symbol = oSymbol;
            Rate = oJson.FundingRate;
            Next = Util.NextFundingRate(8);
        }
        public DateTime Next { get; private set; }

        public decimal Rate { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.FundingRate; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IFundingRate)) return;
            IFundingRate oFunding = (IFundingRate)oMessage;
            Next = oFunding.Next;
            Rate = oFunding.Rate;
        }

        public static IFundingRate? Parse( IFuturesExchange oExchange, JToken? oToken)
        {
            if (oToken == null) return null;
            BitunixFundingRateJson? oJson = oToken.ToObject<BitunixFundingRateJson>();
            if (oJson == null) return null;
            IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(oJson.Symbol);
            if (oSymbol == null) return null;
            return new BitunixFundingRate(oSymbol, oJson);
        }
    }
}

using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Crypto.Futures.Exchanges.WebsocketModel;

namespace Crypto.Futures.Exchanges.Mexc.Data
{

    internal class MexcFundingRateWs
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("rate")]
        public decimal FundingRate { get; set; } = 0m;
    }
    internal class MexcFundingRateJson
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; } = string.Empty;
        [JsonProperty("fundingRate")]
        public decimal FundingRate { get; set; } = 0m;
        [JsonProperty("maxFundingRate")]
        public decimal MaxFundingRate { get; set; } = 0m;
        [JsonProperty("minFundingRate")]
        public decimal MinFundingRate { get; set; } = 0m;
        [JsonProperty("collectCycle")]
        public int CollectCycle { get; set; } = 0;
        [JsonProperty("nextSettleTime")]
        public long NextSettleTime { get; set; } = 0L;
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; } = 0L;
    }

    /// <summary>
    /// Funding rate implementation for Mexc
    /// </summary>
    internal class MexcFundingRate : IFundingRate
    {
        public MexcFundingRate( IFuturesSymbol oSymbol, MexcFundingRateJson oJson ) 
        { 
            Symbol = oSymbol;
            Rate = oJson.FundingRate;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oJson.NextSettleTime);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            Next = dDate;
        }

        public MexcFundingRate(IFuturesSymbol oSymbol, MexcTickerJson oJson)
        {
            Symbol = oSymbol;
            Rate = oJson.FundingRate;
            Next = Util.NextFundingRate(8);
        }

        public MexcFundingRate(IFuturesSymbol oSymbol, MexcFundingRateWs oWs)
        {
            Symbol = oSymbol;
            Rate = oWs.FundingRate;
            Next = Util.NextFundingRate(8);
        }
        public WsMessageType MessageType { get => WsMessageType.FundingRate; }
        public IFuturesSymbol Symbol { get; }

        public DateTime Next { get; private set; }

        public decimal Rate { get; private set; }

        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IFundingRate)) return;
            IFundingRate oFunding = (IFundingRate)oMessage;
            Next = oFunding.Next;
            Rate = oFunding.Rate;
        }

        public static IFundingRate? Parse(IFuturesExchange oExchange, JToken? oJson, bool bTicker)
        {
            if (oJson == null) return null;
            if (!bTicker)
            {
                var oFundingJson = oJson.ToObject<MexcFundingRateJson>();
                if (oFundingJson == null) return null;
                var oSymbol = oExchange.SymbolManager.GetSymbol(oFundingJson.Symbol);
                if (oSymbol == null) return null;
                return new MexcFundingRate(oSymbol, oFundingJson);
            }
            else
            {
                var oTickerJson = oJson.ToObject<MexcTickerJson>();
                if (oTickerJson == null) return null;
                var oSymbol = oExchange.SymbolManager.GetSymbol(oTickerJson.Symbol);
                if (oSymbol == null) return null;
                return new MexcFundingRate(oSymbol, oTickerJson);
            }
        }


        public static IWebsocketMessage[]? ParseWs(IFuturesExchange oExchange, string? strSymbol, JToken? oData)
        {

            if (oData == null) return null;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();
            if (oData is JArray)
            {
                foreach (JToken oData2 in oData)
                {
                    MexcFundingRateWs? oWs = oData2.ToObject<MexcFundingRateWs>();
                    if (oWs == null) continue;
                    string strSymbolReal = oWs.Symbol;
                    if (string.IsNullOrEmpty(strSymbolReal))
                    {
                        if (strSymbol == null) continue;
                        strSymbolReal = strSymbol;
                    }
                    IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(strSymbolReal);
                    if (oSymbol == null) continue;

                    aResult.Add(new MexcFundingRate(oSymbol, oWs));
                }
            }
            else
            {
                MexcFundingRateWs? oWs = oData.ToObject<MexcFundingRateWs>();
                if (oWs == null) return null;
                string strSymbolReal = oWs.Symbol;
                if (string.IsNullOrEmpty(strSymbolReal))
                {
                    if (strSymbol == null) return null;
                    strSymbolReal = strSymbol;
                }
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetSymbol(strSymbolReal);
                if (oSymbol == null) return null;
                aResult.Add(new MexcFundingRate(oSymbol, oWs));
            }
            return aResult.ToArray();

        }
    }
}

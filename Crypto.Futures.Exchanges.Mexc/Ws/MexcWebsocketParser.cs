using Crypto.Futures.Exchanges.Mexc.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Ws
{
    /// <summary>
    /// Ws parser
    /// </summary>
    internal class MexcWebsocketParser : IWebsocketParser
    {
        private const string PING_METHOD = "ping";
        private const string BAR_METHOD = "kline";
        private const string TICKER_METHOD = "ticker";
        private const string FUNDING_METHOD = "funding.rate";
        private const string METHOD_SUBSCRIBE = "sub.";
        private const string METHOD_UNSUBSCRIBE = "unsub.";

        // private const string CHANNEL_TRADE = "push.deal";
        private const string CHANNEL_TICKER = "push.ticker";
        private const string CHANNEL_FUNDING = "push.funding.rate";
        private const string CHANNEL_KLINE = "push.kline";

        public int PingSeconds { get => 20; }

        public MexcWebsocketParser(IFuturesExchange exchange)
        {
            Exchange = exchange;
        }

        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            MexcMessage? oMessage = JsonConvert.DeserializeObject<MexcMessage>(strMessage);
            if (oMessage == null) return null;

            if (oMessage.Channel == CHANNEL_TICKER)
            {
                return MexcTicker.ParseWs(Exchange, oMessage.Data);
            }
            else if (oMessage.Channel == CHANNEL_FUNDING)
            {
                return MexcFundingRate.ParseWs(Exchange, oMessage.Symbol, oMessage.Data);    
            }
            else if( oMessage.Channel == CHANNEL_KLINE)
            { 
                return null;
            }
            return null;
        }

        public IFuturesExchange Exchange { get; }
        public string ParsePing()
        {
            MexcMethod oMethod = new MexcMethod(PING_METHOD, null);
            return JObject.FromObject(oMethod).ToString();
        }

        public string ParsePong()
        {
            throw new NotImplementedException();
        }

        public int MaxSubscriptions { get => 30; }

        public string[]? ParseSubscription(IFuturesSymbol[] aSymbols, WsMessageType eSubscriptionType)
        {
            throw new NotImplementedException("Mexc does not support multiple subscriptions at once. Use ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType) instead.");
        }
        public string ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            throw new NotImplementedException();    
        }
        /*
        /// <summary>
        /// Returns subscribe messages  for websockets
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame)
        {
            List<string> aResult = new List<string>();
            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                // Bar subscription
                var oParamBar = new BarSubscriptionParam(oSymbol, eFrame);
                JObject oParamBarJson = JObject.FromObject(oParamBar);
                MexcMethod oMethodBar = new MexcMethod($"{METHOD_SUBSCRIBE}{BAR_METHOD}", oParamBarJson);
                string strMethodBar = JsonConvert.SerializeObject(oMethodBar);
                aResult.Add(strMethodBar);
                // Trades subscription
                var oSymbolParam = new SymbolSubscription(oSymbol);
                JObject oSymbolJson = JObject.FromObject(oSymbolParam);
                MexcMethod oMethodTrade = new MexcMethod($"{METHOD_SUBSCRIBE}{TICKER_METHOD}", oSymbolJson);
                string strMethodTrade = JsonConvert.SerializeObject(oMethodTrade);
                aResult.Add(strMethodTrade);
                // Funding rate
                MexcMethod oMethodFunding = new MexcMethod($"{METHOD_SUBSCRIBE}{FUNDING_METHOD}", oSymbolJson);
                string strMethodFunding = JsonConvert.SerializeObject(oMethodFunding);
                aResult.Add(strMethodFunding);

            }
            return aResult.ToArray();
        }
        */
    }

}

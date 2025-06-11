using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.MexcWs
{
    /// <summary>
    /// Ws parser
    /// </summary>
    internal class MexcWebsocketParser : IWebsocketParser
    {
        private const string PING_METHOD = "ping";
        private const string BAR_METHOD = "kline";
        private const string METHOD_SUBSCRIBE = "sub.";
        private const string METHOD_UNSUBSCRIBE = "unsub.";

        public int PingSeconds { get => 20; }

        public MexcWebsocketParser(IFuturesExchange exchange)
        {
            Exchange = exchange;
        }

        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            throw new NotImplementedException();
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
                aResult.Add(JObject.FromObject(oMethodBar).ToString());
                // Trades subscription


            }
            return aResult.ToArray();
        }
    }

}

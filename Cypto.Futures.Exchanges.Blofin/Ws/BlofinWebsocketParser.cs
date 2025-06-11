using Crypto.Futures.Exchanges.Blofin.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Cypto.Futures.Exchanges.Blofin.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Ws
{
    internal class BlofinWebsocketParser : IWebsocketParser
    {

        private const string PING = "ping";
        private const string PONG = "pong";
        public int PingSeconds { get => 20; }

        public BlofinWebsocketParser(IFuturesExchange oExchange)
        {
            Exchange = oExchange;
        }


        public IFuturesExchange Exchange { get; }

        /// <summary>
        /// Parse message into object
        /// </summary>
        /// <param name="strMessage"></param>
        /// <returns></returns>
        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            if (strMessage == null || strMessage == PONG) return null;
            BlofinMessage? oMessage = JsonConvert.DeserializeObject<BlofinMessage>(strMessage);
            if (oMessage == null) return null;
            if( oMessage.Event != null ) return null;
            if( oMessage.Argument == null ) return null;
            if( oMessage.Argument.Channel == BlofinSubscription.CHANNEL_TRADES )
            {
                return BlofinTrade.ParseWs(Exchange, oMessage.Data);
            }
           
            else if( oMessage.Argument.Channel == BlofinSubscription.CHANNEL_FUNDING )
            {
                return BlofinFundingRate.ParseWs(Exchange, oMessage.Data);
            }
            return null;
        }

        public string ParsePing()
        {
            return PING;
        }

        public string ParsePong()
        {
            throw new NotImplementedException();
        }

        public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame)
        {
            List<string> result = new List<string>();   
            foreach (IFuturesSymbol oSymbol in aSymbols)
            {
                BlofinSubscription oSubs = new BlofinSubscription(true, oSymbol, BarTimeframe.M1);
                string strSub = JsonConvert.SerializeObject(oSubs, Formatting.Indented);    
                result.Add(strSub);
            }
            return result.ToArray();
        }
    }
}

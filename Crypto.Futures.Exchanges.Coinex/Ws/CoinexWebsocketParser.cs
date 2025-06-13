using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Ws
{
    internal class CoinexWebsocketParser : IWebsocketParser
    {
        private static int m_nGlobalId = 1;

        private const string METHOD_STATE = "state.update";
        private const string METHOD_DEALS = "deals.update";
        public CoinexWebsocketParser(IFuturesExchange oExchange) 
        { 
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        public int PingSeconds { get => 30; }

        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            CoinexMessage? oMessage = JsonConvert.DeserializeObject<CoinexMessage>(strMessage);
            if (oMessage == null) return null;
            if( oMessage.Method == null || oMessage.Data == null ) return null;
            if( oMessage.Method == METHOD_STATE )
            {
                return CoinexFundingRate.ParseWs(Exchange, oMessage.Data);
            }
            else if( oMessage.Method == METHOD_DEALS )
            {
                return CoinexTrade.Parse(Exchange, oMessage.Data);
            }
            return null;
        }

        public string ParsePing()
        {
            CoinexSubscribeJson oJson = new CoinexSubscribeJson(m_nGlobalId++, "server.ping");

            string strResult = JsonConvert.SerializeObject(oJson);

            return strResult;
        }

        public string ParsePong()
        {
            throw new NotImplementedException();
        }

        public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame)
        {
            List<string> aResult = new List<string>();
            CoinexSubscribeJson oJsonFunding = new CoinexSubscribeJson(m_nGlobalId++, aSymbols, CoinexSubscribeJson.METHOD_FUNDING);
            aResult.Add(JsonConvert.SerializeObject( oJsonFunding));

            CoinexSubscribeJson oJsonTrade = new CoinexSubscribeJson(m_nGlobalId++, aSymbols, CoinexSubscribeJson.METHOD_DEALS);
            aResult.Add(JsonConvert.SerializeObject(oJsonTrade));

            return aResult.ToArray();
        }
    }
}

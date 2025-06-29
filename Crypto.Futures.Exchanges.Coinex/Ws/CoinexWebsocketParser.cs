using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Ws
{
    internal class CoinexWebsocketParser : IWebsocketParser
    {
        private static int m_nGlobalId = 1;

        private const string METHOD_STATE   = "state.update";
        private const string METHOD_BBO     = "bbo.update";
        public CoinexWebsocketParser(IFuturesExchange oExchange) 
        { 
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        public int PingSeconds { get => 30; }
        public int MaxSubscriptions { get => 500; }

        private ConcurrentDictionary<int, IWebsocketSubscription> m_aPendingSubscriptions = new ConcurrentDictionary<int, IWebsocketSubscription>();

        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            CoinexMessage? oMessage = JsonConvert.DeserializeObject<CoinexMessage>(strMessage);
            if (oMessage == null) return null;
            if( oMessage.Method == null || oMessage.Data == null )
            {
                if( oMessage.Id != null && oMessage.Id.Value > 0 )
                {
                    if (m_aPendingSubscriptions.TryRemove(oMessage.Id.Value, out IWebsocketSubscription? oSubscription))
                    {
                        return new IWebsocketMessage[] { oSubscription };
                    }
                }
                return null;
            }
            if( oMessage.Method == METHOD_STATE )
            {
                return CoinexStateJson.ParseWs(Exchange, oMessage.Data);
            }
            else if( oMessage.Method == METHOD_BBO )
            {
                return CoinexOrderbookPrice.ParseWs(Exchange, oMessage.Data);
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

        public string[]? ParseSubscription(IFuturesSymbol[] aSymbols, WsMessageType eSubscriptionType)
        {
            throw new NotImplementedException("Coinex does not support multiple subscriptions at once. Use ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType) instead.");
        }
        public string? ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            string? strResult = null;   
            switch( eSubscriptionType )
            {
                case WsMessageType.OrderbookPrice:
                    CoinexSubscribeJson oJsonOrderbook = new CoinexSubscribeJson(m_nGlobalId++, new IFuturesSymbol[] { oSymbol }, CoinexSubscribeJson.METHOD_BBO);
                    strResult = JsonConvert.SerializeObject(oJsonOrderbook);
                    m_aPendingSubscriptions.TryAdd(oJsonOrderbook.Id, new BaseSubscription(eSubscriptionType, oSymbol));
                    break;
                case WsMessageType.FundingRate:
                    CoinexSubscribeJson oJsonFunding = new CoinexSubscribeJson(m_nGlobalId++, new IFuturesSymbol[] { oSymbol }, CoinexSubscribeJson.METHOD_FUNDING);
                    strResult = JsonConvert.SerializeObject(oJsonFunding);
                    m_aPendingSubscriptions.TryAdd(oJsonFunding.Id, new BaseSubscription(eSubscriptionType, oSymbol));
                    break;
                case WsMessageType.LastPrice:
                    break;
            }
            return strResult;
        }

        /*
        public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame)
        {
            List<string> aResult = new List<string>();
            CoinexSubscribeJson oJsonFunding = new CoinexSubscribeJson(m_nGlobalId++, aSymbols, CoinexSubscribeJson.METHOD_FUNDING);
            aResult.Add(JsonConvert.SerializeObject( oJsonFunding));

            CoinexSubscribeJson oJsonTrade = new CoinexSubscribeJson(m_nGlobalId++, aSymbols, CoinexSubscribeJson.METHOD_BBO);
            aResult.Add(JsonConvert.SerializeObject(oJsonTrade));

            return aResult.ToArray();
        }
        */
    }
}

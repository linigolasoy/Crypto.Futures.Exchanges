using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Ws
{
    internal class BingxSubscriptionJson
    {
        // {"id":"e745cd6d-d0f6-4a70-8d5a-043e4c741b40","reqType": "sub","dataType":"BTC-USDT@trade"}
        private Guid m_oGuid;

        private const string SUBSCRIBE = "sub";
        private const string UNSUBSCRIBE = "unsub";
        public BingxSubscriptionJson(IFuturesSymbol oSymbol, bool bSubscribe, WsMessageType eType) 
        {
            m_oGuid = Guid.NewGuid();
            Id = m_oGuid.ToString();    
            RequestType = (bSubscribe? SUBSCRIBE: UNSUBSCRIBE);
            string strDataType = string.Empty;

            switch (eType)
            {
                case WsMessageType.Trade:
                    strDataType = $"{oSymbol.Symbol}@trade";
                    break;
                default:
                    break;
            }
            DataType = strDataType;
        }


        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("reqType")]
        public string RequestType { get; set; }
        [JsonProperty("dataType")]
        public string DataType { get; set; }
    }

}

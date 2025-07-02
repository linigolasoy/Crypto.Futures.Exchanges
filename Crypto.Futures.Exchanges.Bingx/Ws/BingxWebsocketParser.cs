using Crypto.Futures.Exchanges.Bingx.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Ws
{


    internal class BingxMessageJson
    {
        [JsonProperty("code")]
        public int? Code { get; set; } = null;
        [JsonProperty("dataType")]
        public string? DataType { get; set; } = null;
        [JsonProperty("id")]
        public string? Id { get; set; } = null;
        [JsonProperty("data")]
        public JToken? Data { get; set; } = null;



    }

    internal class PingMessage : IWebsocketMessage
    {
        public WsMessageType MessageType { get => WsMessageType.Ping; }

        public IFuturesSymbol Symbol => throw new NotImplementedException();

        public void Update(IWebsocketMessage oMessage)
        {
            throw new NotImplementedException();
        }
    }
    internal class BingxWebsocketParser : IWebsocketParser
    {


        private ConcurrentDictionary<string, IWebsocketSubscription> m_aPendingSubscriptions = new ConcurrentDictionary<string, IWebsocketSubscription>();  
        public BingxWebsocketParser( IFuturesMarket oMarket ) 
        { 
            Exchange = oMarket.Exchange;    
        }
        public IFuturesExchange Exchange { get; }

        public int PingSeconds { get => -1; }

        public int MaxSubscriptions { get => 180; }

        private const string PING = "Ping";
        private const string PONG = "Pong";

        private enum BingxChannels
        {
            lastPrice,
            bookTicker
        }

        public IWebsocketMessageBase[]? ParseMessage(string strMessage)
        {
            try
            {
                if( strMessage.Length < 20 && strMessage.Equals(PING))
                     
                {
                    return new IWebsocketMessageBase[] { new PingMessage() };   
                }
                BingxMessageJson? oJson = JsonConvert.DeserializeObject<BingxMessageJson>(strMessage);  
                if (oJson == null) return null;
                if ( !string.IsNullOrEmpty(oJson.DataType))
                {
                    string[] aSplit = oJson.DataType.Split('@');    
                    if( aSplit.Length != 2 ) return null;
                    if (aSplit[1] == BingxChannels.lastPrice.ToString())
                    {
                        return BingxLastPrice.Parse(Exchange, aSplit[0], oJson.Data);
                    }
                    else if(aSplit[1] == BingxChannels.bookTicker.ToString())
                    {
                        return BingxOrderbookPrice.Parse(Exchange, aSplit[0], oJson.Data);
                    }
                }
                else if( !string.IsNullOrEmpty(oJson.Id))
                {
                    if(m_aPendingSubscriptions.TryRemove(oJson.Id, out IWebsocketSubscription? oSub))
                    {
                        return new IWebsocketMessageBase[] { oSub };
                    }
                }
                else if (oJson.Code.HasValue && oJson.Code.Value != 0)
                {
                    // Error message
                    if (this.Exchange.Logger != null)
                    {
                        this.Exchange.Logger.Error($"Bingx. Error code {oJson.Code.Value} in message: {strMessage}");
                    }
                }
            }
            catch ( Exception ex) 
            { 
                if( this.Exchange.Logger != null )
                {
                    this.Exchange.Logger.Error("Bingx. Error parsing message", ex);
                }
            }
            return null;
        }

        public string ParsePing()
        {
            throw new NotImplementedException();
        }

        public string ParsePong()
        {
            return PONG;
        }


        public string[]? ParseSubscription(IFuturesSymbol[] aSymbols, WsMessageType eSubscriptionType)
        {
            throw new NotImplementedException("Bingx does not support bulk subscriptions. Please use ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType) instead.");
        }

        public string? ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            string? strResult = null;
            switch (eSubscriptionType)
            {
                case WsMessageType.FundingRate:
                    break;
                case WsMessageType.LastPrice:
                    {
                        BingxSubscriptionJson oSub = new BingxSubscriptionJson(oSymbol, true, WsMessageType.LastPrice);
                        strResult = JsonConvert.SerializeObject(oSub, Formatting.Indented);
                        m_aPendingSubscriptions.TryAdd(oSub.Id, new BaseSubscription(eSubscriptionType, oSymbol));
                    }
                    break;
                case WsMessageType.OrderbookPrice:
                    {
                        BingxSubscriptionJson oSub = new BingxSubscriptionJson(oSymbol, true, WsMessageType.OrderbookPrice);
                        strResult = JsonConvert.SerializeObject(oSub, Formatting.Indented);
                        m_aPendingSubscriptions.TryAdd(oSub.Id, new BaseSubscription(eSubscriptionType, oSymbol));
                    }
                    break;
            }
            return strResult;
        }

        /*
        public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame)
        {
            List<string> aResult = new List<string>();  
            foreach (var oSymbol in aSymbols)
            {
                BingxSubscriptionJson oJsonPrice = new BingxSubscriptionJson(oSymbol, true, WsMessageType.LastPrice);
                string strMsgPrice = JsonConvert.SerializeObject(oJsonPrice, Formatting.Indented);
                aResult.Add(strMsgPrice);

                BingxSubscriptionJson oJsonBook = new BingxSubscriptionJson(oSymbol, true, WsMessageType.OrderbookPrice);
                string strMsgBook = JsonConvert.SerializeObject(oJsonBook, Formatting.Indented);
                aResult.Add(strMsgBook);

            }
            return aResult.ToArray();   
        }
        */
    }
}

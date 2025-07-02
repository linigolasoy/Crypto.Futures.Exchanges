using Crypto.Futures.Exchanges.Blofin.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Cypto.Futures.Exchanges.Blofin.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin.Ws
{

    // {"event":"subscribe","arg":{"channel":"tickers","instId":"1000000BABYDOGE-USDT"}}
    internal class BlofinWebsocketParser : IWebsocketParser
    {

        private const string PING = "ping";
        private const string PONG = "pong";
        public int PingSeconds { get => 20; }
        public int MaxSubscriptions { get => 200; }

        public BlofinWebsocketParser(IFuturesExchange oExchange)
        {
            Exchange = oExchange;
        }


        public IFuturesExchange Exchange { get; }


        /// <summary>
        /// Event subscription message
        /// </summary>
        /// <param name="oJson"></param>
        /// <returns></returns>
        private IWebsocketMessageBase[]? ParseEvent( BlofinMessage oJson )
        {
            if(oJson.Event == null || oJson.Argument == null) return null;
            if (oJson.Event != "subscribe") return null;
            List<IWebsocketMessageBase> aResult = new List<IWebsocketMessageBase>();    
            WsMessageType eType = WsMessageType.Trade;
            if (oJson.Argument.Channel == BlofinSubscription.CHANNEL_TICKERS)
            {
                eType = WsMessageType.Ticker;
            }
            else if (oJson.Argument.Channel == BlofinSubscription.CHANNEL_FUNDING)
            {
                eType = WsMessageType.FundingRate;
            }
            else return null;
            if( oJson.Argument.Symbol == null) return null; 
            IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(oJson.Argument.Symbol);
            if (oSymbol == null) return null;
            if( eType == WsMessageType.Ticker )
            {
                aResult.Add(new BaseSubscription(WsMessageType.OrderbookPrice, oSymbol));
                aResult.Add(new BaseSubscription(WsMessageType.LastPrice, oSymbol));
            }
            else
            {
                aResult.Add(new BaseSubscription(eType, oSymbol));
            }


            return aResult.ToArray();   
        }
        /// <summary>
        /// Parse message into object
        /// </summary>
        /// <param name="strMessage"></param>
        /// <returns></returns>
        public IWebsocketMessageBase[]? ParseMessage(string strMessage)
        {
            if (strMessage == null || strMessage == PONG) return null;
            /*
            if (Exchange.Logger != null)
            {
                Exchange.Logger.Info($"Received {Exchange.ExchangeType.ToString()} ({strMessage})");
            }
            */

            BlofinMessage? oMessage = JsonConvert.DeserializeObject<BlofinMessage>(strMessage);
            if (oMessage == null) return null;
            if( oMessage.Event != null ) return ParseEvent(oMessage);
            if( oMessage.Argument == null ) return null;
            List<IWebsocketMessage> aResult = new List<IWebsocketMessage>();    
            if( oMessage.Argument.Channel == BlofinSubscription.CHANNEL_TICKERS )
            {
                return BlofinTicker.ParseWs(Exchange, oMessage.Data);
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

        public string[]? ParseSubscription(IFuturesSymbol[] aSymbols, WsMessageType eSubscriptionType)
        {
            throw new NotImplementedException("Blofin does not support multiple subscriptions at once. Use ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType) instead.");
        }

        public string? ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            string? strResult = null;   
            switch ( eSubscriptionType )
            {
                case WsMessageType.FundingRate:
                    {
                        BlofinSubscription oSub = new BlofinSubscription(true, oSymbol, eSubscriptionType);                       
                        strResult = JsonConvert.SerializeObject(oSub, Formatting.Indented);
                    }
                    break;
                case WsMessageType.LastPrice:
                    break;
                case WsMessageType.OrderbookPrice:
                    {
                        BlofinSubscription oSub = new BlofinSubscription(true, oSymbol, WsMessageType.Ticker);
                        strResult = JsonConvert.SerializeObject(oSub, Formatting.Indented);
                    }
                    break;
            }
            return strResult;
        }

        /*
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
        */
    }
}

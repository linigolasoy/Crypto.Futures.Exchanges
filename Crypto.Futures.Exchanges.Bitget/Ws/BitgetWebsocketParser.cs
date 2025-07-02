using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Ws
{
    internal class BitgetWebsocketParser : IWebsocketParser
    {
        private const string PING = "ping";
        private const string PONG = "pong";

        public BitgetWebsocketParser(IFuturesExchange oExchange) 
        { 
            Exchange = oExchange;    
        }
        public IFuturesExchange Exchange { get; }

        public int PingSeconds { get => 30; }
        public int MaxSubscriptions { get => 500; }

        public IWebsocketMessageBase[]? ParseMessage(string strMessage)
        {
            if (strMessage == PONG) return null;
            BitgetMessage? oMessage = JsonConvert.DeserializeObject<BitgetMessage>(strMessage);
            if (oMessage == null) return null;
            if( oMessage.Action == null || oMessage.Data == null)
            {
                if( oMessage.Event != null && oMessage.Event == "subscribe" && oMessage.Argument != null)
                {
                    if( oMessage.Argument.Channel == ChannelType.ticker.ToString())
                    {
                        IFuturesSymbol? oSymbolSub = Exchange.SymbolManager.GetSymbol(oMessage.Argument.Symbol);
                        if (oSymbolSub == null) return null;
                        return new IWebsocketMessage[] { new BaseSubscription(WsMessageType.OrderbookPrice, oSymbolSub) };
                    }
                    return null;
                }
            }
            if( oMessage.Argument == null ) return null;
            string strSymbol = oMessage.Argument.Symbol;
            IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(strSymbol);
            if (oSymbol == null) return null;
            if( oMessage.Argument.Channel == ChannelType.ticker.ToString() )
            {
                return BitgetTicker.ParseWs(oSymbol, oMessage.Data);
            }
            /*
            else if( oMessage.Argument.Channel == ChannelType.trade.ToString() ) 
            {
                return BitgetTrade.Parse(oSymbol, oMessage.Data);    
            }
            */
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
            throw new NotImplementedException("Bitget does not support multiple subscriptions at once. Use ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType) instead.");
        }
        public string? ParseSubscription(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            string? strResult = null;   
            switch (eSubscriptionType)
            {
                case WsMessageType.OrderbookPrice:
                    BitgetSubscriptionJson oSubOrderbook = new BitgetSubscriptionJson(ChannelType.ticker, true, new IFuturesSymbol[] { oSymbol });
                   
                    strResult = JsonConvert.SerializeObject(oSubOrderbook, Formatting.Indented);
                    break;
                case WsMessageType.FundingRate:
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
            // Tickers
            BitgetSubscriptionJson oSubTicker = new BitgetSubscriptionJson(ChannelType.ticker, true, aSymbols);
            string strSubTicker = JsonConvert.SerializeObject(oSubTicker, Formatting.Indented); 
            aResult.Add(strSubTicker);

            return aResult.ToArray();
        }
        */
    }
}

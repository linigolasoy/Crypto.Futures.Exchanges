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

        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            if (strMessage == PONG) return null;
            BitgetMessage? oMessage = JsonConvert.DeserializeObject<BitgetMessage>(strMessage);
            if (oMessage == null) return null;
            if( oMessage.Action == null || oMessage.Data == null) return null;
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

        public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame)
        {
            List<string> aResult = new List<string>();
            // Tickers
            BitgetSubscriptionJson oSubTicker = new BitgetSubscriptionJson(ChannelType.ticker, true, aSymbols);
            string strSubTicker = JsonConvert.SerializeObject(oSubTicker, Formatting.Indented); 
            aResult.Add(strSubTicker);

            /*
            BitgetSubscriptionJson oSubTrade = new BitgetSubscriptionJson(ChannelType.trade, true, aSymbols);
            string strSubTrade = JsonConvert.SerializeObject(oSubTrade, Formatting.Indented);
            aResult.Add(strSubTrade);
            */
            return aResult.ToArray();
        }
    }
}

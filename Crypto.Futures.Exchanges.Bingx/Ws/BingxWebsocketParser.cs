using Crypto.Futures.Exchanges.Bingx.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using System;
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

        public BingxWebsocketParser( IFuturesMarket oMarket ) 
        { 
            Exchange = oMarket.Exchange;    
        }
        public IFuturesExchange Exchange { get; }

        public int PingSeconds { get => -1; }

        private const string PING = "Ping";
        private const string PONG = "Pong";

        private enum BingxChannels
        { 
            trade
        }

        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            try
            {
                if( strMessage.Length < 20 && strMessage.Equals(PING))
                     
                {
                    return new IWebsocketMessage[] { new PingMessage() };   
                }
                BingxMessageJson? oJson = JsonConvert.DeserializeObject<BingxMessageJson>(strMessage);  
                if (oJson == null) return null;
                if ( !string.IsNullOrEmpty(oJson.DataType))
                {
                    string[] aSplit = oJson.DataType.Split('@');    
                    if( aSplit.Length != 2 ) return null;
                    if (aSplit[1] == BingxChannels.trade.ToString())
                    {
                        return BingxTrade.Parse(Exchange, aSplit[0], oJson.Data);
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

        public string[] ParseSubscription(IFuturesSymbol[] aSymbols, BarTimeframe eFrame)
        {
            List<string> aResult = new List<string>();  
            foreach (var oSymbol in aSymbols)
            {
                BingxSubscriptionJson oJson = new BingxSubscriptionJson(oSymbol, true, WsMessageType.Trade);
                string strMsg = JsonConvert.SerializeObject(oJson, Formatting.Indented);
                aResult.Add(strMsg);
            }
            return aResult.ToArray();   
        }
    }
}

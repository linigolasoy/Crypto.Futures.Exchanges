using Crypto.Futures.Exchanges.Bitmart.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Ws
{
    internal class BitmartWebsocketParser : IWebsocketParser
    {
        private const string PING = "ping";

        public BitmartWebsocketParser( IFuturesExchange oExchange ) 
        { 
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        public int PingSeconds { get => 18; }

        public IWebsocketMessage[]? ParseMessage(string strMessage)
        {
            BitmartMessage? oMessage = JsonConvert.DeserializeObject<BitmartMessage?>( strMessage );
            if ( oMessage == null ) return null;
            if (oMessage.Data == null || oMessage.Group == null) return null;
            string[] aSplit = oMessage.Group.Split(':');
            string strChannel = aSplit[0];
            string strSymbol = aSplit[1];   
            IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol( strSymbol );
            if ( oSymbol == null ) return null;
            if (strChannel == BitmartSubscription.CHANNEL_FUNDING)
            {
                IWebsocketMessage? oWsMessage = BitmartFundingRate.Parse(oSymbol, oMessage.Data);
                if (oWsMessage == null) return null;
                return new IWebsocketMessage[] { oWsMessage };
            }
            else if (strChannel == BitmartSubscription.CHANNEL_TRADE)
            {
                return BitmartTrade.Parse(oSymbol, oMessage.Data);  
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
            List<string> aResult = new List<string>();

            // Subscribe to funding rates
            BitmartSubscription oSubFunding = new BitmartSubscription(aSymbols, BitmartSubscription.CHANNEL_FUNDING, true);
            string strSubFunding = JsonConvert.SerializeObject(oSubFunding, Formatting.Indented);
            aResult.Add(strSubFunding);

            // Subscribe to funding trades
            BitmartSubscription oSubTrade = new BitmartSubscription(aSymbols, BitmartSubscription.CHANNEL_TRADE, true);
            string strSubTrade = JsonConvert.SerializeObject(oSubTrade, Formatting.Indented);
            aResult.Add(strSubTrade);
            return aResult.ToArray();
        }
    }
}

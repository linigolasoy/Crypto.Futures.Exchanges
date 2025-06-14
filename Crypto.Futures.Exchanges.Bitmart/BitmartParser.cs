using Crypto.Futures.Exchanges.Bitmart.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    internal class BitmartParser : ICryptoRestParser
    {
        public BitmartParser(IFuturesExchange exchange)
        {
            Exchange = exchange;
        }   
        public IFuturesExchange Exchange { get; }

        public string? ErrorToMessage(int nError)
        {
            return nError.ToString();
        }

        public IFuturesSymbol? ParseSymbols(JToken? oJson)
        {
            return BitmartSymbol.Parse(Exchange, oJson);
        }
        public IFundingRate? ParseFundingRate(JToken? oJson)
        {
            return BitmartFundingRate.Parse(Exchange, oJson);
        }

        public IBar? ParseBar(IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oJson)
        {
            return BitmartBar.Parse(Exchange, oSymbol, eFrame, oJson); 
        }

        public ITicker? ParseTicker( JToken? oJson)
        {
            return BitmartTicker.Parse(Exchange, oJson);
        }
        public IBalance? ParseBalance(JToken? oJson)
        {
            return BitmartBalance.Parse(Exchange, oJson);
        }
    }
}

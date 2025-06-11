using Crypto.Futures.Exchanges.Bingx.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx
{
    internal class BingxParser : ICryptoRestParser
    {
        public BingxParser(IFuturesExchange oExchange)
        {
            Exchange = oExchange;   
        }
        public IFuturesExchange Exchange { get; }

        public string? ErrorToMessage(int nError)
        {
            return nError.ToString();
        }

        public IFuturesSymbol? ParseSymbols(JToken? oJson)
        {
            return BingxSymbol.Parse(this.Exchange, oJson);
        }
        public IFundingRate? ParseFundingRate(JToken? oJson)
        {
            return BingxFundingRate.Parse(this.Exchange, oJson);
        }

        public IBar? ParseBar(IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oJson) 
        {
            return BingxBar.Parse(Exchange, oSymbol, eFrame, oJson);
        }
        public ITicker? ParseTicker(JToken? oJson)
        {
            return BingxTicker.Parse(this.Exchange, oJson);
        }
    }
}

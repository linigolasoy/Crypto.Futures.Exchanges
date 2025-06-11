using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Crypto.Futures.Exchanges;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cypto.Futures.Exchanges.Blofin.Data;
using Crypto.Futures.Exchanges.Blofin.Data;

namespace Cypto.Futures.Exchanges.Blofin
{
    internal class BlofinParser : ICryptoRestParser
    {
        public BlofinParser(IFuturesExchange oExchange)
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
            return BlofinSymbol.Parse(Exchange, oJson);
        }


        public IFundingRate? ParseFundingRate(JToken? oJson)
        {
            return BlofinFundingRate.Parse(Exchange, oJson);
        }

        public ITicker? ParseTicker(JToken? oJson)
        {
            return BlofinTicker.Parse(Exchange, oJson);
        }
    }
}

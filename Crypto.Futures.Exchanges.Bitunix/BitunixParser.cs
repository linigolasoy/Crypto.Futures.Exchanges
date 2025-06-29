using Crypto.Futures.Exchanges.Bitunix.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitunix
{
    internal class BitunixParser : ICryptoRestParser
    {
        public BitunixParser(IFuturesExchange oExchange)
        {
            Exchange = oExchange;
        }
        public IFuturesExchange Exchange { get; }

        public string? ErrorToMessage(int nError)
        {
            return nError.ToString();
        }

        public IFuturesSymbol? ParseSymbols( JToken? oToken )
        {
            return BitunixSymbol.Parse(Exchange, oToken);
        }

        public IFundingRate? ParseFundingRate(JToken? oToken)
        {
            return BitunixFundingRate.Parse(Exchange, oToken);
        }
        public ITicker? ParseTicker(JToken? oToken)
        {
            return BitunixTicker.Parse(Exchange, oToken);
        }
    }
}

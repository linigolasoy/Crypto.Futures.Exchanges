using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    internal class BitgetParser : ICryptoRestParser
    {
        public BitgetParser(IFuturesExchange exchange)
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
            return BitgetSymbol.Parse(this.Exchange, oJson);
        }

        public IFundingRate? ParseFundingRate(JToken? oJson)
        {
            return BitgetFundingRate.Parse(this.Exchange, oJson);
        }

        public ITicker? ParseTicker(JToken? oJson)
        {
            return BitgetTicker.Parse(this.Exchange, oJson);
        }
        public IBar? ParseBar(IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oJson)
        {
            return BitgetBar.Parse(Exchange, oSymbol, eFrame, oJson);
        }
    }
}

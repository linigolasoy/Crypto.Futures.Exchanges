using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc
{
    internal class MexcParser : ICryptoRestParser
    {

        public MexcParser(IFuturesExchange oExchange)
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
            return MexcSymbol.Parse(this.Exchange, oJson);
        }


        public IFundingRate? ParseFundingRate(JToken? oJson, bool bTicker)
        {
            if (oJson == null) return null;
            return MexcFundingRate.Parse(this.Exchange, oJson, bTicker); 
        }


        public IBar[]? ParseBars(IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oJson)
        {
            return MexcBar.Parse(oSymbol, eFrame, oJson);
        }

    }
}

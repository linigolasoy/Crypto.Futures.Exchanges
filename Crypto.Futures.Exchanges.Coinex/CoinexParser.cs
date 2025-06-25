using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex
{
    internal class CoinexParser : ICryptoRestParser
    {
        public CoinexParser(IFuturesExchange exchange)
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
            return CoinexSymbol.Parse(this.Exchange, oJson);
        }

        public IFundingRate? ParseFundingRate(JToken? oJson)
        {
            return CoinexFundingRate.Parse(this.Exchange, oJson);   
        }

        public ITicker? ParseTicker(JToken? oJson)
        {
            return CoinexTicker.Parse(this.Exchange, oJson);
        }
        public IBar? ParseBar( IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oJson)
        {
            return CoinexBar.Parse(Exchange, oSymbol, eFrame, oJson);
        }

        public IBalance? ParseBalance(JToken? oJson)
        {
            return CoinexBalance.Parse(Exchange, oJson);
        }
        public IPosition? ParsePosition(JToken? oJson)
        {
            return CoinexPosition.Parse(Exchange, oJson);
        }
    }
}

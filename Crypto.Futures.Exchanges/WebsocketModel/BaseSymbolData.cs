using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    internal class BaseSymbolData : IWebsocketSymbolData
    {
        public BaseSymbolData( IFuturesSymbol oSymbol ) 
        { 
            Symbol = oSymbol;   
        }
        public IFuturesSymbol Symbol { get; }

        public ITrade? LastTrade { get; internal set; } = null;

        public IFundingRate? FundingRate { get; internal set; } = null;
    }
}

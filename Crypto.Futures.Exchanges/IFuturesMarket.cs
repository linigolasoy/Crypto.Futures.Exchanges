using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges
{

    /// <summary>
    /// IFuturesMarket represents the market interface for a cryptocurrency futures exchange.
    /// Rest calls for public market data, such as order book, trades, and ticker information.
    /// Websockets for real-time market data updates.   
    /// </summary>
    public interface IFuturesMarket
    {
        public IFuturesExchange Exchange { get; }


        // Funding rates getting
        public Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol);
        public Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols = null);
        // Tickers 
    }
}

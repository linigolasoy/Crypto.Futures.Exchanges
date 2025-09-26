using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges
{
    /// <summary>
    /// IFuturesHistory represents the historical data interface for a cryptocurrency futures exchange.
    /// Ticks, bars, funding history, etc.  
    /// </summary>
    public interface IFuturesHistory
    {
        // TODO: Methods
        public IFuturesExchange Exchange { get; }   

        public Task<IBar[]?> GetBars( IFuturesSymbol oSymbol, BarTimeframe eFrame, DateTime dFrom, DateTime dTo);

        public Task<IBar[]?> GetBars(IFuturesSymbol[] aSymbols, BarTimeframe eFrame, DateTime dFrom, DateTime dTo);

        public Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol oSymbol, DateTime dFrom, DateTime dTo);
        public Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[] aSymbols, DateTime dFrom, DateTime dTo);
    }
}

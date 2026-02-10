using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface
{
    /// <summary>
    /// Trader
    /// </summary>
    public interface ICryptoTrader
    {


        public decimal Money { get; }
        public decimal Leverage { get; }
        public int OrderTimeout { get; set; } // Timeout for orders in seconds   
        public ICommonLogger Logger { get; }
        public IAccountWatcher AccountWatcher { get; }
        public IQuoter Quoter { get; }

        public Task<IPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null);

        public Task<bool> Close(IPosition oPosition, decimal? nPrice = null);

        public Task<IOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal nPrice);
        public Task<bool> ClosePendingOrders(IFuturesSymbol oSymbol);

        public Task<bool> PutLeverage(IFuturesSymbol oSymbol);


    }
}

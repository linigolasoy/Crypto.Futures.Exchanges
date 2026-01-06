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

        public delegate bool MustCancelOrderDelegate();

        public decimal Money { get; }
        public decimal Leverage { get; }
        public int OrderTimeout { get; set; } // Timeout for orders in seconds   
        public ICommonLogger Logger { get; }
        public IBalance[] Balances { get; }

        public ICryptoPosition[] PositionsActive { get; }
        public ICryptoPosition[] PositionsClosed { get; }

        public void InitBalances();
        public Task<ICryptoPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, MustCancelOrderDelegate? OncheckCancel = null, decimal? nPrice = null);
        public Task<ICryptoPosition?> OpenFillOrKill(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null);

        public Task<bool> Close(ICryptoPosition oPosition, MustCancelOrderDelegate? OncheckCancel = null, decimal? nPrice = null);
        public Task<bool> CloseFillOrKill(ICryptoPosition oPosition, decimal? nPrice = null);

        public Task<bool> PutLeverage(IFuturesSymbol oSymbol);


    }
}

using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges
{
    /// <summary>
    /// IFuturesAccount represents the account management interface user account, such as balances, positions, and orders.
    /// Also includes websockets for account updates.
    /// </summary>
    public interface IFuturesAccount
    {
        // TODO: Methods
        public IFuturesExchange Exchange { get; }


        public Task<IBalance[]?> GetBalances();

        public Task<decimal?> GetLeverage(IFuturesSymbol oSymbol);
        public Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage);
    }
}

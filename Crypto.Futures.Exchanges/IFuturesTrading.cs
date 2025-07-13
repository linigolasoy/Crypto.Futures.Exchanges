using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges
{
    /// <summary>
    /// Trading actions on exchange
    /// </summary>
    public interface IFuturesTrading
    {
        // TODO: Methods
        public IFuturesExchange Exchange { get; }

        public Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null);
        public Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null);

        public Task<bool> CloseOrders(IFuturesSymbol oSymbol);
    }
}

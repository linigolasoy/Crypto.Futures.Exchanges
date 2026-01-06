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
    /// Account watcher, balances ,positions and orders
    /// </summary>
    public interface IAccountWatcher
    {
        public IFuturesExchange[] Exchanges { get; }

        public delegate void OnBalanceChangeDelegate(IBalance oBalance);
        public delegate void OnPositionChangeDelegate(ICryptoPosition oPosition);
        public delegate void OnOrderChangeDelegate(IOrder oOrder);

        public event OnBalanceChangeDelegate? OnBalanceChange;
        public event OnPositionChangeDelegate? OnPositionChange;
        public event OnOrderChangeDelegate? OnOrderChange;

        public IBalance[] GetBalances();
        public ICryptoPosition[] GetPositions();
        public IOrder[] GetOrders();

        public Task Start();
        public Task Stop();
    }
}

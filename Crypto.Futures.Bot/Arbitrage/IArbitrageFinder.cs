using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{
    /// <summary>
    /// Find chances and subscribe to websockets
    /// </summary>
    internal interface IArbitrageFinder
    {
        public ITradingBot Bot { get; }

        public string[] SubscribedCurrencies { get; }

        public Task<IArbitrageChance[]?> FindValidChances();
        public Task<bool> Start();
        public Task<bool> Stop();

    }
}

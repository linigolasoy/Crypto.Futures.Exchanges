using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.Arbitrage
{
    /// <summary>
    /// Currency finder
    /// </summary>
    public interface IArbitrageCurrencyFinder
    {
        public ICryptoBot Bot { get; }

        public IArbitrageCurrency[] Currencies { get; }

        public Task<bool> Start();
        public Task<bool> Stop();
    }
}

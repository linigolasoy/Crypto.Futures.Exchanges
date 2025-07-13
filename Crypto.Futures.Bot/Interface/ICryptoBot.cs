using Crypto.Futures.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface
{

    /// <summary>
    /// Crypto bot
    /// </summary>
    public interface ICryptoBot
    {
        public IExchangeSetup Setup { get; }
        public ICommonLogger Logger { get; }

        public IFuturesExchange[] Exchanges { get; }
        public ICryptoTrader Trader { get; }

        public CancellationToken CancelToken { get; }
        public Task<bool> Start();

        public Task<bool> Stop();

        public Task<bool> Close(string strCurrency);
        public Task<bool> CloseAll();
    }
}

using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot
{
    /// <summary>
    /// Trading bot
    /// </summary>
    public interface ITradingBot
    {
        public IExchangeSetup Setup { get; }    
        public ICommonLogger Logger { get; }

        public IFuturesExchange[] Exchanges { get; }
        public ICryptoTrader Trader { get; }  
        public Task<bool> Start();

        public Task<bool> Stop();
    }
}

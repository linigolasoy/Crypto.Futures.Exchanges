using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.Arbitrage
{
    /// <summary>
    /// Arbitrage bot
    /// </summary>
    public interface IArbitrageBot: ICryptoBot
    {
        public IArbitrageChanceManager ChanceManager { get; }   
    }
}

using Crypto.Futures.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.Arbitrage
{

    /// <summary>
    /// Eval real chances
    /// </summary>
    internal interface IArbitrageChanceEvaluator
    {
        public IExchangeSetup Setup { get; }
        public IArbitrageChance[] ToChances(IArbitrageCurrency[] aCurrencies);
    }
}

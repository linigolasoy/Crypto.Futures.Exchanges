using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage.Model
{

    /// <summary>
    /// Chance counter to prevent profitless trades.
    /// </summary>
    internal class ChanceCounter
    {

        public string Currency { get; }
        public int Count { get; private set; }

        public decimal Profit { get; private set; }

        public ChanceCounter(IArbitrageChance oChance)
        {
            Currency = oChance.Currency;
            Count = 1;
            Profit = (oChance.Profit == null ? 0: oChance.Profit.Value);
        }

        public void Update(IArbitrageChance oChance )
        {
            Count++;
            Profit += (oChance.Profit == null ? 0 : oChance.Profit.Value);
        }
    }
}

using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{

    /// <summary>
    /// Pending chance for currency finder
    /// </summary>
    internal interface IPendingChance
    {
        public string Currency { get; }

        public IFuturesSymbol[] Symbols { get; }

        public decimal Percentage { get; }  
    }
}

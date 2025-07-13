using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.Arbitrage
{

    /// <summary>
    /// Possible arbitrage currency found by tickers
    /// </summary>
    public interface IArbitrageCurrency
    {
        public string Currency { get; }

        public IOrderbookPrice[] OrderbookPrices { get; }
    }
}

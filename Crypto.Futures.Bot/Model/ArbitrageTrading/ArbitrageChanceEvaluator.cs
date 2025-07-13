using Crypto.Futures.Bot.Interface.Arbitrage;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{

    /// <summary>
    /// Chance evaluator
    /// </summary>
    internal class ArbitrageChanceEvaluator : IArbitrageChanceEvaluator
    {
        public ArbitrageChanceEvaluator(IExchangeSetup oSetup) 
        {
            Setup = oSetup;
        }

        public IExchangeSetup Setup { get; }
        public IArbitrageChance[] ToChances(IArbitrageCurrency[] aCurrencies)
        {
            List<IArbitrageChance> aResult = new List<IArbitrageChance>();  
            foreach (var oCurrency in aCurrencies)
            {
                IOrderbookPrice oShortBook = oCurrency.OrderbookPrices.OrderByDescending(p=> p.BidPrice).First();
                IOrderbookPrice oLongBook = oCurrency.OrderbookPrices.OrderBy(p => p.AskPrice).First();
                if (oShortBook.Symbol.Exchange.ExchangeType == oLongBook.Symbol.Exchange.ExchangeType) continue;

                IArbitrageChance oChance = new ArbitrageChance(Setup, oCurrency.Currency, oLongBook, oShortBook);
                if (!oChance.Check()) continue;
                aResult.Add(oChance);

            }
            return aResult.ToArray();   
        }
    }
}

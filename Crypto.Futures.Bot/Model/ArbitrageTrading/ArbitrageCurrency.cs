using Crypto.Futures.Bot.Interface.Arbitrage;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{
    internal class ArbitrageCurrency : IArbitrageCurrency
    {
        public ArbitrageCurrency( string strCurrency, IOrderbookPrice[] aPrices) 
        { 
            Currency = strCurrency;
            OrderbookPrices = aPrices;
        }
        public string Currency { get; }

        public IOrderbookPrice[] OrderbookPrices { get; }
    }
}

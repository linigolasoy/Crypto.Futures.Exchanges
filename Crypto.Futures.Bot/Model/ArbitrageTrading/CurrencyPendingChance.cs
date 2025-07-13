using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{
    internal class CurrencyPendingChance : IPendingChance
    {
        public CurrencyPendingChance( string strCurrency, IFuturesSymbol[] aSymbols, decimal nPercent) 
        { 
            Currency = strCurrency;
            Symbols = aSymbols;
            Percentage = nPercent;
        }
        public string Currency { get; }

        public IFuturesSymbol[] Symbols { get; }

        public decimal Percentage { get; }

        public override string ToString()
        {
            return $"{Currency}.- Percent {Math.Round(Percentage, 2)}%";
        }
    }
}

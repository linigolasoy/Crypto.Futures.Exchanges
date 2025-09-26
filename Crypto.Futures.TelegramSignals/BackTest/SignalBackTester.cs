using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.TelegramSignals.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.TelegramSignals.BackTest
{
    internal class SignalBackTester : ISignalBackTester
    {
        public SignalBackTester(ISignalScanner signalScanner, IFuturesExchange exchange, decimal money, decimal operationCount)
        {
            SignalScanner = signalScanner;
            Exchange = exchange;
            Money = money;
            OperationCount = operationCount;
        }
        public ISignalScanner SignalScanner { get; }

        public IFuturesExchange Exchange { get; }

        public decimal Money { get; }

        public decimal OperationCount { get; }


        /// <summary>
        /// Run signal scanner
        /// </summary>
        /// <param name="dFrom"></param>
        /// <param name="dTo"></param>
        /// <param name="nMoney"></param>
        /// <param name="nOperations"></param>
        /// <returns></returns>
        public async Task<ISignalBackTesterResult?> Run(DateTime dFrom, DateTime dTo, decimal nMoney, int nOperations)
        {
            ISignal[]? aSignals = await SignalScanner.GetHistory(dFrom);
            if (aSignals == null || aSignals.Length <= 0)
            {
                return null; // no signals found
            }
            aSignals = aSignals.Where(p => p.SignalDate >= dFrom.Date && p.SignalDate <= dTo.Date.AddDays(1).AddSeconds(-1)).OrderBy(p=> p.SignalDate).ToArray(); 
            BackTestResult oResult = new BackTestResult(this, dFrom, dTo);  

            decimal nMoneyPerOperation = nMoney / (decimal)nOperations; // money per operation   

            foreach ( var oSignal in aSignals )
            {
                // Get the bars for the signal
                DateTime dToSignal = oSignal.SignalDate.Date.AddDays(15);
                if (dTo >= DateTime.Today) dToSignal = DateTime.Today;
                IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == oSignal.Currency && p.Quote == "USDT");
                if (oSymbol == null) continue; // no symbol found for this signal
                IBar[]? aBars = await Exchange.History.GetBars(oSymbol, BarTimeframe.M15, oSignal.SignalDate, dToSignal);
                if (aBars == null || aBars.Length <= 0) continue; // no bars found for this signal
                ISignalBackTesterChance oChance = oResult.NewSignal(oSignal, oSymbol, nMoneyPerOperation);
                if (oChance == null) continue; // no chance created, skip this signal
                foreach (var oBar in aBars)
                {
                    if (!oResult.Update(oChance, oBar)) break; // update the chance with the bar data
                }
            }

            return oResult; // return the result of the backtest
        }
    }
}

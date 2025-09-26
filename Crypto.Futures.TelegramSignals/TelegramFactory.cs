using Crypto.Futures.Exchanges;
using Crypto.Futures.TelegramSignals.BackTest;
using Crypto.Futures.TelegramSignals.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.TelegramSignals
{
    public interface TelegramFactory
    {

        public static ISignalScanner CreateSignalScanner(IExchangeSetup setup, ICommonLogger logger, SignalType eType)
        {
            return new TelegramMessageHandler(setup, logger, eType);
        }

        public static ISignalBackTester CreateBackTester(IFuturesExchange oExchange, ISignalScanner oScanner, decimal nMoney, int nOperations)
        {
            return new SignalBackTester(oScanner, oExchange, nMoney, nOperations); 
        }
    }
}

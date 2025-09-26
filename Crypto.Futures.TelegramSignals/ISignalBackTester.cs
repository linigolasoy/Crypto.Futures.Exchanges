using Crypto.Futures.Exchanges;
using Crypto.Futures.TelegramSignals.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.TelegramSignals
{

    /// <summary>
    /// Signal backtester chance interface
    /// </summary>
    public interface ISignalBackTesterChance
    {
        public ISignal Signal { get; } // signal that this chance is based on   
        public DateTime? DateOpen { get; } // date of the chance
        public DateTime? DateClose { get; } // date of the chance
        public decimal EntryPrice { get; } // entry price of the chance 
        public decimal ExitPrice { get; } // exit price of the chance
        public decimal Profit { get; } // profit of the chance
        public decimal Pnl { get; } // profit of the chance unrealized  
        public decimal Quantity { get; } // quantity of the chance  
    }


    /// <summary>
    /// Signal backtester snapshot interface    
    /// </summary>
    public interface ISignalBackTesterSnapshot
    {
        public ISignalBackTesterResult Result { get; }
        public DateTime Date { get; } // date of the snapshot   
        public decimal Money { get; } // money at the time of the snapshot
        public decimal UnRealized { get; } // profit at the time of the snapshot
        public int OpenCount { get; } // number of open operations at the time of the snapshot  
    }


    /// <summary>
    /// Signal backtester result interface  
    /// </summary>
    public interface ISignalBackTesterResult
    {
        public ISignalBackTester BackTester { get; }

        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int Count { get; }   
        public int Won { get; } 
        public decimal Profit { get; }  

        public ISignalBackTesterChance[] Chances { get; }
        public ISignalBackTesterSnapshot[] Snapshots { get; } // snapshots of the backtester at different points in time
    }

    /// <summary>
    /// Signal backtester
    /// </summary>
    public interface ISignalBackTester
    {
        public ISignalScanner SignalScanner { get; }
        public IFuturesExchange Exchange { get; } // exchange to use for backtesting
        public decimal Money { get; }
        public decimal OperationCount { get; }

        public Task<ISignalBackTesterResult?> Run(DateTime dFrom, DateTime dTo, decimal nMoney, int nOperations);

    }
}

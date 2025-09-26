using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.TelegramSignals.Signals
{
    internal class BaseSignal: ISignal
    {
        public BaseSignal(DateTime signalDate, string currency, SignalType type, bool bLong, decimal[] entries, decimal[] takeProfit, decimal stopLoss, bool bCanEnter)
        {
            SignalDate = signalDate;
            Currency = currency;
            Type = type;
            Entries = entries;
            TakeProfit = takeProfit;
            StopLoss = stopLoss;
            CanEnter = bCanEnter;
            IsLong = bLong;
        }

        public DateTime SignalDate { get; }

        public string Currency { get; }

        public SignalType Type { get; }
        public bool IsLong { get; }

        public decimal[] Entries { get; }

        public decimal[] TakeProfit { get; }

        public decimal StopLoss { get; }
        public bool CanEnter { get; }

        public DateTime? CloseDate { get; internal set; } = null; // no close date by default, can be set later if needed
    }
}

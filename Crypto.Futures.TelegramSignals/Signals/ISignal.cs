using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.TelegramSignals.Signals
{

    public enum SignalType
    {
        EveningTraderMidCap,
        AfibieCryptoSignals
    }
    
    /// <summary>
    /// Futures signal
    /// </summary>
    public interface ISignal
    {
        public DateTime SignalDate { get; }
        public string Currency { get; }
        public bool IsLong { get; }
        public SignalType Type { get; }
        public decimal[] Entries { get; }
        public decimal[] TakeProfit { get; }
        public decimal StopLoss { get; }
        public bool CanEnter { get; }   
        public DateTime? CloseDate { get; } 
    }
}

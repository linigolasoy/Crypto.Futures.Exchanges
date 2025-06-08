using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{

    /// <summary>
    /// KLines (bars in graphic)
    /// </summary>
    public enum BarTimeframe
    {
        M1 = 1,
        M5 = 5,
        M15 = 15,
        M30 = 30,
        H1 = 60,
        H4 = 240,
        D1 = 1440
    }
    public interface IBar
    {
        public IFuturesSymbol Symbol { get; }   
        public BarTimeframe Timeframe { get; }
        public DateTime DateTime { get; }
        public decimal Open { get; }    
        public decimal Close { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Volume { get; }
    }
}

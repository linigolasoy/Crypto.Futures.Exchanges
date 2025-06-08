using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    internal class FuturesBar : IBar
    {
        public FuturesBar( 
                IFuturesSymbol oSymbol,
                BarTimeframe eFrame
            ) 
        { 
            Symbol = oSymbol;
            Timeframe = eFrame;
        }
        public IFuturesSymbol Symbol { get; }

        public BarTimeframe Timeframe { get; }

        public DateTime DateTime { get; internal set; } = DateTime.MinValue;

        public decimal Open { get; internal set; } = 0;

        public decimal Close { get; internal set; } = 0;

        public decimal High { get; internal set; } = 0;

        public decimal Low { get; internal set; } = 0;

        public decimal Volume { get; internal set; } = 0;
    }
}

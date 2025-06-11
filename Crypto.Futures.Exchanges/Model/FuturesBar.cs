using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    public class FuturesBar : IBar
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

        public DateTime DateTime { get; set; } = DateTime.MinValue;

        public decimal Open { get; set; } = 0;

        public decimal Close { get; set; } = 0;

        public decimal High { get; set; } = 0;

        public decimal Low { get; set; } = 0;

        public decimal Volume { get; set; } = 0;
    }
}

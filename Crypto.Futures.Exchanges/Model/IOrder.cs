using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    public interface IOrder
    {
        public IFuturesSymbol Symbol { get; }
    }
}

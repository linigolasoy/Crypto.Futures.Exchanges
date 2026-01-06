using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface
{
    public interface IVirtualPosition
    {
        public int Id { get; }

        public IFuturesSymbol Symbol { get; }
        public bool IsLong { get; }
        public decimal Quantity { get; }
    }
}

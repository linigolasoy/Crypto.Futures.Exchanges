using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    public interface ICryptoRestParser
    {
        public IFuturesExchange Exchange { get; }

        public string? ErrorToMessage(int nError);
    }
}

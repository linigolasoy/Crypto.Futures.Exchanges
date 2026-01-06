using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface
{
    /// <summary>
    /// Quoter interface
    /// </summary>
    public interface IQuoter
    {
        public IFuturesExchange[] Exchanges { get; }

        public Task<decimal?> GetLongPrice(IFuturesSymbol oSymbol, decimal nQuantity = 0);
        public Task<decimal?> GetShortPrice(IFuturesSymbol oSymbol, decimal nQuantity = 0);

        public decimal? GetBestQuantity(IFuturesSymbol[] aSymbols, decimal nPrice, decimal nMoney);
    }
}

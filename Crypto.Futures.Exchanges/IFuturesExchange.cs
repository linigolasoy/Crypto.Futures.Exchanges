using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges
{
    /// <summary>
    /// Exchange represents a cryptocurrency futures exchange.  
    /// </summary>
    public interface IFuturesExchange
    {
        public IExchangeSetup Setup { get; }

        public ICommonLogger? Logger { get; }
        public ExchangeType ExchangeType { get; }

        // Market rest calls and public websocket calls
        public IFuturesMarket Market { get; }
        // History, tickers, funding, etc.  
        public IFuturesHistory History { get; }

        // Trading rest calls 
        public IFuturesTrading Trading { get; }
        // Account and balance and private websocket calls
        public IFuturesAccount Account { get; }
        // Symbol management, symbol info, etc.
        public IFuturesSymbolManager SymbolManager { get; }

        public Task<IFuturesSymbol[]?> RefreshSymbols();
    }
}

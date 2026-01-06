using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot
{

    public interface ITraderPosition
    {
        public IFuturesSymbol Symbol { get; }   
        public long Id { get; } 
        public bool IsLong {  get; } 
        public decimal Volume { get; }  
        public decimal PriceOpen { get; }
        public decimal PriceClose { get; }
        public decimal ActualPrice { get; } // Current price of the position, if open   

        public decimal Profit { get;  }
        public DateTime DateOpen { get; }
        public DateTime? DateClose { get; }

        public IPosition? Position { get; }
        public bool Update();
    }

    /// <summary>
    /// Trading interface
    /// </summary>
    public interface ITrader
    {

        public decimal Money { get; }   
        public decimal Leverage { get; }
        public int OrderTimeout { get; set; } // Timeout for orders in seconds   
        // public ITradingBot Bot { get; }
        public ICommonLogger Logger { get; }

        public IBalance[] Balances { get; }
        public ITraderPosition[] ActivePositions { get; }
        public ITraderPosition[] ClosedPositions { get; }
        public Task<ITraderPosition?> Open( IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null);
        public Task<bool> Close(ITraderPosition oPosition, decimal? nPrice = null);

        public Task<bool> PutLeverage(IFuturesSymbol oSymbol);
        public bool Update();

    }
}

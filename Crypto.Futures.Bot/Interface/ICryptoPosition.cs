using Crypto.Futures.Exchanges.Model;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface
{

    /// <summary>
    /// Crypto position
    /// </summary>
    public interface ICryptoPosition
    {

        public int Id { get; }
        public ICryptoTrader Trader { get; }    
        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice OrderbookPrice { get; }

        public bool IsLong {  get; }    
        public decimal Profit { get; }  
        public decimal Quantity { get; }    

        public decimal LastPrice { get; }

        public IOrder OrderOpen { get; }
        public IOrder? OrderClose { get; }

        public bool Update();
    }
}

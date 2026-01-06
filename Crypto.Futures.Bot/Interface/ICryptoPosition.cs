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
    public interface ICryptoPosition: IVirtualPosition
    {

        public ICryptoTrader Trader { get; }    

        public IOrderbookPrice OrderbookPrice { get; }

        public decimal Profit { get; }  

        public decimal LastPrice { get; }

        public IOrder OrderOpen { get; }
        public IOrder? OrderClose { get; }

        public bool Update();
    }
}

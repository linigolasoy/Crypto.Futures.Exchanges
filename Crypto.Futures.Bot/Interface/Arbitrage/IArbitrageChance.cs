using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.Arbitrage
{

    /// <summary>
    /// Real arbitrage chance
    /// </summary>
    public interface IArbitrageChance
    {
        public IExchangeSetup Setup { get; }    
        public string Currency { get; }

        public IOrderbookPrice OrderbookLong { get; }
        public IOrderbookPrice OrderbookShort { get; }

        public decimal PriceLong { get; }
        public decimal PriceShort { get; }
        public decimal Percent {  get; }    
        public bool Check();
    }
}

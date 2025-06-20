using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{
    /// <summary>
    /// Symbol data of a chance
    /// </summary>
    internal interface IArbitrageSymbolData
    {
        public IArbitrageChance Chance { get; }
        public IFuturesSymbol Symbol { get; }

        public IWebsocketSymbolData? WsSymbolData { get; }

        public ITraderPosition? Position { get; set; }

    }
}

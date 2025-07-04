using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage.Model
{
    internal class ArbitrageSymbolData : IArbitrageSymbolData
    {
        public ArbitrageSymbolData(IArbitrageChance oChance, IWebsocketSymbolData oSymbolData ) 
        { 
            Chance = oChance;
            Symbol = oSymbolData.Symbol;
            WsSymbolData = oSymbolData;
        }
        public IArbitrageChance Chance { get; }
        public decimal? DesiredPriceOpen { get; set; } = null; // Desired price to open position
        public decimal? DesiredPriceClose { get; set; } = null; // Desired price to close position

        public IFuturesSymbol Symbol { get; }

        public IWebsocketSymbolData? WsSymbolData { get; }

        public ITraderPosition? Position { get; set ; } =null;
    }
}

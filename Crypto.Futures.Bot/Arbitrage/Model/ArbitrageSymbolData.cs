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

        public IFuturesSymbol Symbol { get; }

        public IWebsocketSymbolData? WsSymbolData { get; }

        public ITraderPosition? Position { get; set ; } =null;
    }
}

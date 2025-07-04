﻿using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Data stored on websocket for each symbol
    /// </summary>
    public interface IWebsocketSymbolData
    {
        public DateTime LastUpdate { get; }
        public IFuturesSymbol Symbol { get; }

        public ILastPrice? LastPrice { get; }    
        public IOrderbookPrice? LastOrderbookPrice { get; }
        // public ITrade? LastTrade { get; }   
        // public ITicker? Ticker { get; }
        public IFundingRate? FundingRate { get; }   
    }
}

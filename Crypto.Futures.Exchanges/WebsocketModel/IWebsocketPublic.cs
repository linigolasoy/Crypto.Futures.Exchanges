using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{




    /// <summary>
    /// Market websockets
    /// </summary>
    public interface IWebsocketPublic
    {
        public IFuturesMarket Market { get; }
        public BarTimeframe Timeframe { get; set; }
        public IWebsocketParser Parser { get; }
        public IWebsocketDataManager DataManager { get; }
        public string Url { get; }
        public bool Started { get; }
        public Task<bool> Start();
        public Task<bool> Stop();

        // public Task Send(string strSend);
        public Task<bool> Subscribe(IFuturesSymbol oSymbol);
        public Task<bool> Subscribe(IFuturesSymbol[] aSymbols);
        public Task<bool> UnSubscribe(IFuturesSymbol oSymbol);
        public Task<bool> UnSubscribe(IFuturesSymbol[]? aSymbols = null);

        public IFuturesSymbol[] SubscribedSymbols { get; }  
    }




}

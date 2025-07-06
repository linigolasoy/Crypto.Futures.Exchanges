using BingX.Net.Clients;
using BingX.Net.Interfaces.Clients;
using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using CryptoExchange.Net.Authentication;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx
{

    /// <summary>
    /// Bingx Futures Exchange implementation.  
    /// </summary>
    public class BingxFutures : IFuturesExchange
    {

        private IBingXRestClient m_oRestClient;

        public BingxFutures(IExchangeSetup oSetup, ICommonLogger? logger = null)
        {
            Setup = oSetup;
            Logger = logger;
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);
            m_oRestClient = new BingXRestClient();  
            m_oRestClient.SetApiCredentials(new ApiCredentials(ApiKey.ApiKey, ApiKey.ApiSecret));   
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BingxMarket(this); 
            // History = new BingxHistory(this);
            Account = new BingxAccount(this);
            Trading = new BingxTrading(this);
        }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => true; }

        internal IBingXRestClient RestClient { get => m_oRestClient; }
        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BingxFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History { get => throw new NotImplementedException(); }

        public IFuturesTrading Trading { get; }

        public IFuturesAccount Account { get; }

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            {
                var oResult = await m_oRestClient.PerpetualFuturesApi.ExchangeData.GetContractsAsync();
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;
                List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
                foreach (var oSymbol in oResult.Data)
                {
                    if (oSymbol == null) continue;
                    aResult.Add(new BingxSymbol(this, oSymbol));
                }

                SymbolManager.SetSymbols(aResult.ToArray());
                return aResult.ToArray();
            }
            catch ( Exception ex) 
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;    
            }
        }
    }
}

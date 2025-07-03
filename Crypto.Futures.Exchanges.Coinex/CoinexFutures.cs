using CoinEx.Net.Clients;
using CoinEx.Net.Interfaces.Clients;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using CryptoExchange.Net.Authentication;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex
{
    /// <summary>
    /// Coinex Futures Exchange implementation.
    /// </summary>
    public class CoinexFutures : IFuturesExchange
    {

        private ICoinExRestClient m_oRestClient;

        public CoinexFutures( IExchangeSetup oSetup, ICommonLogger? oLogger) 
        {
            Setup = oSetup;
            Logger = oLogger;
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);
            m_oRestClient = new CoinExRestClient();
            m_oRestClient.SetApiCredentials(new ApiCredentials(ApiKey.ApiKey, ApiKey.ApiSecret));
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new CoinexMarket(this);    
            // History = new CoinexHistory(this);
            // Account = new CoinexAccount(this);
        }

        public bool Tradeable { get => true; }

        // internal CryptoRestClient RestClient { get {return new CryptoRestClient(BASE_URL, ApiKey, m_oParser); } }

        internal ICoinExRestClient RestClient { get => m_oRestClient; }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.CoinExFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account => throw new NotImplementedException();

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            {
                var oResult = await RestClient.FuturesApi.ExchangeData.GetSymbolsAsync();   
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;
                List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
                foreach (var oSymbol in oResult.Data)
                {
                    if (oSymbol == null) continue;
                    if( !oSymbol.TradingAvailable) continue; // Skip symbols that are not trading available 
                    aResult.Add(new CoinexSymbol(this, oSymbol));   
                }

                SymbolManager.SetSymbols(aResult.ToArray());
                return aResult.ToArray();
            }
            catch (Exception ex)
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;
            }

        }
    }
}

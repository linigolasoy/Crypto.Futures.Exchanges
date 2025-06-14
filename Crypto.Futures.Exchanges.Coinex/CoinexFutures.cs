using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex
{
    /// <summary>
    /// Coinex Futures Exchange implementation.
    /// </summary>
    public class CoinexFutures : IFuturesExchange
    {

        private CoinexParser m_oParser;
        private const string BASE_URL = "https://api.coinex.com/v2";
        private const string ENDP_SYMBOLS = "/futures/market";
        public CoinexFutures( IExchangeSetup oSetup, ICommonLogger? oLogger) 
        {
            Setup = oSetup;
            Logger = oLogger;
            m_oParser = new CoinexParser(this); 
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);  
            
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new CoinexMarket(this);    
            History = new CoinexHistory(this);
            Account = new CoinexAccount(this);
        }

        public bool Tradeable { get => true; }

        internal CryptoRestClient RestClient { get {return new CryptoRestClient(BASE_URL, ApiKey, m_oParser); } }
        internal CoinexParser Parser { get => m_oParser; }  
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.CoinExFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History { get; }

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account { get; }

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            { 
                var oResult = await RestClient.DoGetArrayParams<IFuturesSymbol?>(ENDP_SYMBOLS, null, p => m_oParser.ParseSymbols(p));
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;
                List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
                foreach (var oSymbol in oResult.Data)
                {
                    if (oSymbol == null) continue;
                    aResult.Add(oSymbol);
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

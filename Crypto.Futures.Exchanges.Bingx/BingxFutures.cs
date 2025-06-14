using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
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
        public const string BASE_URL = "https://open-api.bingx.com";
        private const string ENDP_SYMBOLS = "/openApi/swap/v2/quote/contracts";

        private BingxParser m_oParser;

        public BingxFutures(IExchangeSetup oSetup, ICommonLogger? logger = null)
        {
            Setup = oSetup;
            Logger = logger;
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);
            m_oParser = new BingxParser(this);
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BingxMarket(this); 
            History = new BingxHistory(this);
            Account = new BingxAccount(this);
        }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => true; }

        internal CryptoRestClient RestClient { get { return new CryptoRestClient(BASE_URL, ApiKey, m_oParser); } }
        internal BingxParser Parser { get => m_oParser; }
        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BingxFutures; }

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
            catch ( Exception ex) 
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;    
            }
        }
    }
}

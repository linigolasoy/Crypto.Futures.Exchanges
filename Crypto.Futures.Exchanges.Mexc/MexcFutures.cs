using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;

namespace Crypto.Futures.Exchanges.Mexc
{
    public class MexcFutures : IFuturesExchange
    {

        public const int MAX_TASKS = 20;
        public const string BASE_URL = "https://contract.mexc.com";
        private const string ENDP_SYMBOLS = "/api/v1/contract/detail";

        internal CryptoRestClient m_oRestClient;
        internal MexcParser m_oParser;
        public MexcFutures(IExchangeSetup oSetup, ICommonLogger? logger = null)
        {
            Setup = oSetup;
            Logger = logger;
            m_oParser = new MexcParser(this);
            m_oRestClient = new CryptoRestClient(BASE_URL, m_oParser);
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new MexcMarket(this);
            History = new MexcHistory(this);
        }


        internal CryptoRestClient RestClient { get => m_oRestClient; }
        internal MexcParser Parser { get => m_oParser; }
        public IExchangeSetup Setup { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.MexcFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History { get; }

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account => throw new NotImplementedException();

        public IFuturesSymbolManager SymbolManager { get; }

        /// <summary>
        /// Refreshes the symbols from the exchange.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            { 
                var oResult = await m_oRestClient.DoGetArray<IFuturesSymbol?>(ENDP_SYMBOLS, null, p => m_oParser.ParseSymbols(p));
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

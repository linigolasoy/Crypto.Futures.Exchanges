using Crypto.Futures.Exchanges.Mexc.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json;

namespace Crypto.Futures.Exchanges.Mexc
{
    public class MexcFutures : IFuturesExchange
    {

        public const int MAX_TASKS = 20;
        public const string BASE_URL = "https://contract.mexc.com";
        private const string ENDP_SYMBOLS = "/api/v1/contract/detail";

        // internal MexcParser m_oParser;
        public MexcFutures(IExchangeSetup oSetup, ICommonLogger? logger = null)
        {
            Setup = oSetup;
            Logger = logger;
            // m_oParser = new MexcParser(this);
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);  
            
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new MexcMarket(this);
            History = new MexcHistory(this);
            Account = new MexcAccount(this);
            Trading = new MexcTrading(this);
        }


        internal IApiCaller ApiCaller { get { return new BaseApiCaller(BASE_URL);  } }
        // internal MexcParser Parser { get => m_oParser; }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => false; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.MexcFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History { get; }

        public IFuturesTrading Trading { get; }

        public IFuturesAccount Account { get; }

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
                var oResult = await ApiCaller.GetAsync(ENDP_SYMBOLS);
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                MexcResponse? oResponse = JsonConvert.DeserializeObject<MexcResponse>(oResult.Data.ToString());
                if (oResponse == null || oResponse.Data == null || !oResponse.Success ) return null;

                IFuturesSymbol[]? aResult = MexcSymbol.ParseAll(this, oResponse.Data);
                if (aResult == null || aResult.Length <= 0) return null;
                SymbolManager.SetSymbols(aResult);
                return aResult;
            }
            catch (Exception ex)
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;
            }
        }
    }
}

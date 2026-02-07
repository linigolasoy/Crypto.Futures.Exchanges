using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Rest;
using System.Net.Http.Headers;
using Crypto.Futures.Exchanges.Blofin;

namespace Cypto.Futures.Exchanges.Blofin
{
    public class BlofinFutures : IFuturesExchange
    {

        public const int MAX_TASKS = 20;
        private const string BASE_URL = "https://openapi.blofin.com";
        private const string ENDP_SYMBOLS = "/api/v1/market/instruments";

        // internal BlofinParser m_oParser;

        public BlofinFutures(IExchangeSetup oSetup, ICommonLogger? oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            // m_oParser = new BlofinParser(this);
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);
            
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BlofinMarket(this);
            Account = new BlofinAccount(this);
        }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => false; }


        internal IApiCaller ApiCaller { get { return new BaseApiCaller(BASE_URL); } }
        // internal BlofinParser Parser { get => m_oParser; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BlofinFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account { get; }

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            
            try
            {
                var oResult = await ApiCaller.GetAsync(ENDP_SYMBOLS);
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                IFuturesSymbol[]? aResult = BlofinSymbol.ParseAll(this, oResult.Data);
                if(aResult == null) throw new Exception("Failed to parse symbols from Blofin.");


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

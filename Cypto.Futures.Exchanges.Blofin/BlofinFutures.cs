using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Rest;
using System.Net.Http.Headers;
using Crypto.Futures.Exchanges.Blofin;
using BloFin.Net.Interfaces.Clients;
using BloFin.Net.Clients;
using CryptoExchange.Net.Authentication;

namespace Cypto.Futures.Exchanges.Blofin
{
    public class BlofinFutures : IFuturesExchange
    {

        public static string BASE_URL = "https://openapi.blofin.com";

        // internal BlofinParser m_oParser;
        private IBloFinRestClient m_oRestClient;

        public BlofinFutures(IExchangeSetup oSetup, ICommonLogger? oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            // m_oParser = new BlofinParser(this);
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);

            m_oRestClient = new BloFinRestClient();
            m_oRestClient.SetApiCredentials(new ApiCredentials(ApiKey.ApiKey, ApiKey.ApiSecret, "Cotton12$$")); 

            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BlofinMarket(this);
            Account = new BlofinAccount(this);
        }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => false; }



        public IBloFinRestClient RestClient { get => m_oRestClient; }
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
                var oResult = await RestClient.FuturesApi.ExchangeData.GetSymbolsAsync();
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                List<IFuturesSymbol> aResult = new List<IFuturesSymbol>();
                foreach( var oData in oResult.Data )
                {
                    IFuturesSymbol oSymbol = new BlofinSymbolMine(this, oData);
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

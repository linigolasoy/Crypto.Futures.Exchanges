using Crypto.Futures.Exchanges.Model;
using CryptoExchange.Net.Authentication;
using Toobit.Net.Clients;
using Toobit.Net.Interfaces.Clients;

namespace Crypto.Futures.Exchanges.Toobit
{
    /// <summary>
    /// Toobit Futures Exchange implementation.
    /// </summary>
    public class ToobitFutures: IFuturesExchange
    {
        private IToobitRestClient m_oRestClient;

        public ToobitFutures(IExchangeSetup oSetup, ICommonLogger? oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            ApiKey = Setup.ApiKeys.First(p => p.ExchangeType == this.ExchangeType);
            m_oRestClient = new ToobitRestClient();
            m_oRestClient.SetApiCredentials(new ApiCredentials(ApiKey.ApiKey, ApiKey.ApiSecret));
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            // Market = new CoinexMarket(this);
            // History = new CoinexHistory(this);
            // Account = new CoinexAccount(this);
        }

        public IExchangeSetup Setup { get; }

        public IApiKey ApiKey { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.ToobitFutures; }

        public bool Tradeable { get => true; }

        public IFuturesMarket Market => throw new NotImplementedException();

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account => throw new NotImplementedException();

        public IFuturesSymbolManager SymbolManager { get; }


        /// <summary>
        /// Initial load of symbols from the exchange.  
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            var oResult = await m_oRestClient.UsdtFuturesApi.ExchangeData.GetExchangeInfoAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.ContractSymbols == null || oResult.Data.ContractSymbols.Length <= 0) return null;

            foreach( var oAsset in oResult.Data.ContractSymbols )
            {
                continue;
            }
            throw new NotImplementedException();
        }
    }
}

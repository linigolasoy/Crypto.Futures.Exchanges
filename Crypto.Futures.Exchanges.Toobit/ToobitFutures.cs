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
            Market = new ToobitMarket(this);
            // History = new CoinexHistory(this);
            Account = new ToobitAccount(this);
            Trading = new ToobitTrading(this);
        }

        public IExchangeSetup Setup { get; }

        internal IToobitRestClient RestClient { get => m_oRestClient; }

        public IApiKey ApiKey { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.ToobitFutures; }

        public bool Tradeable { get => true; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading { get; }

        public IFuturesAccount Account { get; }

        public IFuturesSymbolManager SymbolManager { get; }


        /// <summary>
        /// Initial load of symbols from the exchange.  
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            var oResult = await RestClient.UsdtFuturesApi.ExchangeData.GetExchangeInfoAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.ContractSymbols == null || oResult.Data.ContractSymbols.Length <= 0) return null;

            List<IFuturesSymbol> aSymbols = new List<IFuturesSymbol>();
            foreach ( var oAsset in oResult.Data.ContractSymbols )
            {
                if( oAsset.QuoteAsset != "USDT") continue; // Only USDT quote assets for now
                if( oAsset.Underlying == null )
                {
                    continue;
                }
                BaseSymbol oSymbol = new BaseSymbol(
                    this,
                    oAsset.Symbol,
                    oAsset.Underlying,
                    oAsset.QuoteAsset
                    );
                oSymbol.Decimals = -(int) Math.Log10((double)oAsset.BaseAssetPrecision);
                oSymbol.QuantityDecimals = -(int)Math.Log10((double)oAsset.QuotePrecision);
                oSymbol.LeverageMax = 10; //  oAsset.lev;
                oSymbol.FeeMaker = 0.0005m;
                oSymbol.FeeTaker = 0.0006m;
                oSymbol.ContractSize = (oAsset.ContractMultiplier == null ? 1: oAsset.ContractMultiplier.Value);
                aSymbols.Add(oSymbol);
            }

            SymbolManager.SetSymbols(aSymbols.ToArray());
            return aSymbols.ToArray();

        }
    }
}

using Crypto.Futures.Exchanges.Model;
using CryptoExchange.Net.Authentication;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Interfaces.Clients;

namespace Crypto.Futures.Exchanges.Hyperliquidity
{
    /// <summary>
    /// Hyperliquidity Exchange implementation.
    /// </summary>
    public class HyperliquidityExchanges : IFuturesExchange
    {




        public HyperliquidityExchanges(IExchangeSetup setup, ICommonLogger? logger = null)
        {
            RestClient = new HyperLiquidRestClient();

            ApiKey = setup.ApiKeys.First(p => p.ExchangeType == this.ExchangeType);
            ApiCredentials oCred = new ApiCredentials(
                ApiKey.ApiKey,
                ApiKey.ApiSecret
                );

            RestClient.SetApiCredentials(oCred);
            Setup = setup;
            Logger = logger;
            // m_oParser = new HyperParser(this);
            // TODO: Load symbols, initialize components
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  


            Market = new HyperLiquidityMarket(this);
            Account = new HyperLiquidityAccount(this);
            Trading = new HyperLiquidityTrading(this);
        }

        internal IHyperLiquidRestClient RestClient { get; }

        public IExchangeSetup Setup { get; }

        public IApiKey ApiKey { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.Hyperliquidity; }

        public bool Tradeable { get => true; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading { get; }

        public IFuturesAccount Account { get; }

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            {
                var aTickers = await RestClient.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync();
                if( aTickers == null || !aTickers.Success || aTickers.Data == null) return null;
                if( aTickers.Data.ExchangeInfo.Symbols == null || aTickers.Data.ExchangeInfo.Symbols.Length == 0) return null;
                List<IFuturesSymbol> aSymbols = new List<IFuturesSymbol>();
                foreach (var sym in aTickers.Data.ExchangeInfo.Symbols)
                {
                    BaseSymbol oSymbol = new BaseSymbol(
                        this,
                        sym.Name,
                        sym.Name,
                        "USDT"
                        );
                    oSymbol.Decimals = sym.QuantityDecimals;
                    oSymbol.QuantityDecimals = sym.QuantityDecimals;
                    oSymbol.LeverageMax = sym.MaxLeverage;
                    oSymbol.FeeMaker = 0.00015m;
                    oSymbol.FeeTaker = 0.00045m;
                    aSymbols.Add(oSymbol);

                }
                SymbolManager.SetSymbols(aSymbols.ToArray());
                return aSymbols.ToArray();
            }
            catch (Exception ex)
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;
            }
        }
    }
}

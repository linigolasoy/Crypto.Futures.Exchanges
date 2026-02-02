using Crypto.Futures.Exchanges.Hyperliquidity.Parsing;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System.Buffers.Text;

namespace Crypto.Futures.Exchanges.Hyperliquidity
{
    /// <summary>
    /// Hyperliquidity Exchange implementation.
    /// </summary>
    public class HyperliquidityExchanges : IFuturesExchange
    {

        // private HyperParser m_oParser;

        public const string BASE_URL = "https://api.hyperliquid.xyz";
        public const string ENDP_SYMBOLS = "/info";


        public HyperliquidityExchanges(IExchangeSetup setup, ICommonLogger? logger = null)
        {
            Setup = setup;
            Logger = logger;
            ApiKey = setup.ApiKeys.First(p => p.ExchangeType == this.ExchangeType);
            // m_oParser = new HyperParser(this);
            // TODO: Load symbols, initialize components
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  


            Market = new HyperLiquidityMarket(this);
        }

        internal IApiCaller ApiCaller 
        { 
            get 
            { 
                return new BaseApiCaller(BASE_URL); 
            }
        }

        public IExchangeSetup Setup { get; }

        public IApiKey ApiKey { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.Hyperliquidity; }

        public bool Tradeable { get => true; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account => throw new NotImplementedException();

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            {
                PostInfoParams oParams = new PostInfoParams();
                var oResult = await ApiCaller.PostAsync(ENDP_SYMBOLS, oParams);
                if (oResult == null || !oResult.Success || oResult.Data == null ) return null;

                IFuturesSymbol[] aResult = SymbolMetadataParser.ParseSymbols(oResult.Data, this);
                SymbolManager.SetSymbols(aResult.ToArray());
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

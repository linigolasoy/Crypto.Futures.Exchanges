using Crypto.Futures.Exchanges.Bitunix.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Newtonsoft.Json;

namespace Crypto.Futures.Exchanges.Bitunix
{
    public class BitunixFutures: IFuturesExchange
    {
        public const int MAX_TASKS = 20;
        private const string BASE_URL = "https://fapi.bitunix.com/";
        private const string ENDP_SYMBOLS = "/api/v1/futures/market/trading_pairs";

        // internal BitunixParser m_oParser;

        public BitunixFutures(IExchangeSetup oSetup, ICommonLogger? oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            // m_oParser = new BitunixParser(this);
            ApiKey = Setup.ApiKeys.First(p => p.ExchangeType == this.ExchangeType);

            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BitunixMarket(this);
            //Account = new BlofinAccount(this);
        }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => false; }
        internal IApiCaller ApiCaller { get { return new BaseApiCaller(BASE_URL); } }
        // internal BitunixParser Parser { get => m_oParser; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitunixFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History => throw new NotImplementedException();

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account => throw new NotImplementedException();

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            try
            {
                var oResult = await ApiCaller.GetAsync(ENDP_SYMBOLS);
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;

                BitunixResponse? oResponse = JsonConvert.DeserializeObject<BitunixResponse>(oResult.Data);
                if (oResponse == null || !oResponse.IsSuccess() &&  oResponse.data == null) return null;
                IFuturesSymbol[]? aResult = BitunixSymbol.ParseAll(this, oResponse.data);
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

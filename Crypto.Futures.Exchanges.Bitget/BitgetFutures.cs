using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    public class BitgetFutures : IFuturesExchange
    {
        // https://api.bitget.com/api/v2/mix/market/contracts?productType=usdt-futures
        private BitgetParser m_oParser;
        private const string BASE_URL = "https://api.bitget.com/api/v2";
        private const string ENDP_SYMBOLS = "/mix/market/contracts?productType=usdt-futures";

        public BitgetFutures(IExchangeSetup oSetup, ICommonLogger? oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;
            m_oParser = new BitgetParser(this);
            ApiKey = Setup.ApiKeys.First(p=> p.ExchangeType == this.ExchangeType);
            
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BitgetMarket(this);
            History = new BitgetHistory(this);
            Account = new BitgetAccount(this);
        }

        internal CryptoRestClient RestClient { get { return new CryptoRestClient(BASE_URL, ApiKey, m_oParser);  } }
        internal BitgetParser Parser { get => m_oParser; }
        public IExchangeSetup Setup { get; }
        public IApiKey ApiKey { get; }
        public bool Tradeable { get => true; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitgetFutures; }

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
            catch( Exception ex) 
            {
                if (Logger != null) Logger.Error("Error refreshing symbols", ex);
                return null;
            }
        }
    }
}

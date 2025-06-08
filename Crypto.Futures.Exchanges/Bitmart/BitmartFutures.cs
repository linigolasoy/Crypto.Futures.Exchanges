using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    internal class BitmartFutures : IFuturesExchange
    {
        private CryptoRestClient m_oRestClient;
        private BitmartParser m_oParser;
        internal const string BASE_URL = "https://api-cloud-v2.bitmart.com";
        internal const string ENDP_SYMBOLS = "/contract/public/details";

        public BitmartFutures(IExchangeSetup oSetup, ICommonLogger? oLogger = null)
        {
            Setup = oSetup;
            Logger = oLogger;
            m_oParser = new BitmartParser(this);
            m_oRestClient = new CryptoRestClient(BASE_URL, m_oParser);
            SymbolManager = new FuturesSymbolManager();
            var oTask = RefreshSymbols();
            oTask.Wait(); // Wait for the symbols to be loaded  
            Market = new BitmartMarket(this);  
            History = new BitmartHistory(this);
        }

        internal CryptoRestClient RestClient { get=> m_oRestClient; }   
        internal BitmartParser Parser { get => m_oParser; } 
        public IExchangeSetup Setup { get; }

        public ICommonLogger? Logger { get; }

        public ExchangeType ExchangeType { get => ExchangeType.BitmartFutures; }

        public IFuturesMarket Market { get; }

        public IFuturesHistory History { get; }

        public IFuturesTrading Trading => throw new NotImplementedException();

        public IFuturesAccount Account => throw new NotImplementedException();

        public IFuturesSymbolManager SymbolManager { get; }

        public async Task<IFuturesSymbol[]?> RefreshSymbols()
        {
            var oResult = await m_oRestClient.DoGetArray<IFuturesSymbol?>(ENDP_SYMBOLS, "symbols", p => m_oParser.ParseSymbols(p));
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
    }
}

using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitunix
{
    internal class BitunixMarket : IFuturesMarket
    {
        private BitunixFutures m_oExchange;

        private const string ENDP_FUNDING = "/api/v1/futures/market/funding_rate/batch";
        private const string ENDP_TICKERS = "/api/v1/futures/market/tickers";

        public BitunixMarket(BitunixFutures oExchange)
        {
            m_oExchange = oExchange;
            // Websocket = new BitunixWebsocketPublic(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket => throw new NotImplementedException();


        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            /*
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<IFundingRate?>(ENDP_FUNDING, null, p => m_oExchange.Parser.ParseFundingRate(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach (var oFunding in oResult.Data)
            {
                if (oFunding == null) continue;
                aResult.Add(oFunding);
            }
            return aResult.ToArray();
            */
            throw new NotImplementedException();
        }
        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            IFundingRate[]? aRates = await GetFundingRates(new IFuturesSymbol[] { oSymbol });
            if (aRates == null) return null;
            return aRates.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols = null)
        {
            IFundingRate[]? aAll = await GetAllFundingRates();
            if (aAll == null) return null;
            if (aSymbols == null || aSymbols.Length <= 0) return aAll;
            return aAll.Where(p => aSymbols.Any(s => s.Symbol == p.Symbol.Symbol)).ToArray();
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols = null)
        {
            /*
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<ITicker?>(ENDP_FUNDING, null, p => m_oExchange.Parser.ParseTicker(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<ITicker> aResult = new List<ITicker>();
            foreach (var oFunding in oResult.Data)
            {
                if (oFunding == null) continue;
                if( aSymbols != null && aSymbols.Length > 0 && !aSymbols.Any(s => s.Symbol == oFunding.Symbol.Symbol)) continue;
                aResult.Add(oFunding);
            }
            return aResult.ToArray();
            */
            throw new NotImplementedException();
        }
    }
}

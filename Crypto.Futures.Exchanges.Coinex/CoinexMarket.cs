using Crypto.Futures.Exchanges.Coinex.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex
{
    internal class CoinexMarket : IFuturesMarket
    {
        private const string ENDP_FUNDING = "/futures/funding-rate";
        private const string ENDP_TICKERS = "/futures/ticker";
        private CoinexFutures m_oExchange;
        public CoinexMarket( CoinexFutures oExchange)
        {
            m_oExchange = oExchange;
            Websocket = new CoinexWebsocketPublic(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket { get ; }

        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<IFundingRate?>(ENDP_FUNDING, null, p => m_oExchange.Parser.ParseFundingRate(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;
            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach (var f in oResult.Data)
            {
                if( f!= null) aResult.Add(f);   
            }

            return aResult.ToArray();
        }
        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            IFundingRate[]? aAllFunding = await GetAllFundingRates();
            if (aAllFunding == null) return null;
            return aAllFunding.FirstOrDefault(f => f.Symbol.Symbol == oSymbol.Symbol);
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols)
        {
            IFundingRate[]? aAllFunding = await GetAllFundingRates();
            if (aAllFunding == null) return null;
            if( aSymbols == null) return aAllFunding;
            return aAllFunding.Where(f => aSymbols.Any(s => f.Symbol.Symbol == s.Symbol)).ToArray();
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols)
        {
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<ITicker?>(ENDP_TICKERS, null, p => m_oExchange.Parser.ParseTicker(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;
            List<ITicker> aResult = new List<ITicker>();
            foreach (var f in oResult.Data)
            {
                if (f == null) continue;
                if( aSymbols != null)
                {
                    if (!aSymbols.Any(p => p.Symbol == f.Symbol.Symbol)) continue;
                }
                aResult.Add(f);
            }

            return aResult.ToArray();
        }
    }
}

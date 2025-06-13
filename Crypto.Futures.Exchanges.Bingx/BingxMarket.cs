using Crypto.Futures.Exchanges.Bingx.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx
{
    internal class BingxMarket : IFuturesMarket
    {
        private BingxFutures m_oExchange;

        private const string ENDP_FUNDING = "/openApi/swap/v2/quote/premiumIndex";
        private const string ENDP_TICKER = "/openApi/swap/v2/quote/ticker";
        public BingxMarket( BingxFutures oExchange ) 
        { 
            m_oExchange = oExchange;
            Websocket = new BingxWebsocketPublic(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }
        public IWebsocketPublic Websocket { get ; }


        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            var oResult = await m_oExchange.RestClient.DoGetArray<IFundingRate?>(ENDP_FUNDING, null, p => m_oExchange.Parser.ParseFundingRate(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach( var oFunding in oResult.Data )
            {
                if (oFunding == null) continue;
                aResult.Add(oFunding);
            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Get funding rate, single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            IFundingRate[]? aAllFunding = await GetAllFundingRates();
            if (aAllFunding == null) return null;
            return aAllFunding.FirstOrDefault( f => f.Symbol.Symbol == oSymbol.Symbol );  
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols)
        {
            IFundingRate[]? aAllFunding = await GetAllFundingRates();
            if (aAllFunding == null) return null;
            if (aSymbols == null) return aAllFunding;
            return aAllFunding.Where(f => aSymbols.Any( s=> f.Symbol.Symbol == s.Symbol)).ToArray();
        }
        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols)
        {
            var oResult = await m_oExchange.RestClient.DoGetArray<ITicker?>(ENDP_TICKER, null, p => m_oExchange.Parser.ParseTicker(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<ITicker> aResult = new List<ITicker>();
            foreach (var oTicker in oResult.Data)
            {
                if (oTicker == null) continue;
                if( aSymbols != null )
                {
                    if (!aSymbols.Any(p => p.Symbol == oTicker.Symbol.Symbol)) continue;
                }
                aResult.Add(oTicker);
            }
            return aResult.ToArray();
        }
    }
}

using Crypto.Futures.Exchanges.Mexc.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc
{
    /// <summary>
    /// MexcMarket represents the market interface for the Mexc Futures exchange.
    /// </summary>
    internal class MexcMarket : IFuturesMarket
    {

        private const string ENDP_FUNDING_RATE  = "/api/v1/contract/funding_rate/";
        private const string ENDP_TICKER        = "/api/v1/contract/ticker/";
        private MexcFutures m_oExchange;


        public MexcMarket( MexcFutures oExchange ) 
        {
            m_oExchange = oExchange;
            Websocket = new MexcWebsocketPublic(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket { get;}


        /// <summary>
        /// Get funding rates, single symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            /*
            string strEndPoint = $"{ENDP_FUNDING_RATE}{oSymbol.Symbol}";
            var oResult = await m_oExchange.RestClient.DoGetParams<IFundingRate?>(strEndPoint, p => m_oExchange.Parser.ParseFundingRate(p, false));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            return oResult.Data;    
            */
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get funding rates, multiple symbols
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols)
        {
            /*
            string strEndPoint = ENDP_TICKER;
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<IFundingRate?>(strEndPoint, null, p => m_oExchange.Parser.ParseFundingRate(p, true));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;


            List<IFundingRate> aResults = new List<IFundingRate>();
            foreach (var oResultFund in oResult.Data)
            {
                if( oResultFund == null) continue;
                aResults.Add(oResultFund);
            }


            return aResults.ToArray();
            */
            throw new NotImplementedException();
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols)
        {
            /*
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<ITicker?>(ENDP_TICKER, null, p => m_oExchange.Parser.ParseTicker(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<ITicker> aResult = new List<ITicker>();
            foreach (var oTicker in oResult.Data)
            {
                if (oTicker == null) continue;
                if (aSymbols != null)
                {
                    if (!aSymbols.Any(p => p.Symbol == oTicker.Symbol.Symbol)) continue;
                }
                aResult.Add(oTicker);
            }
            return aResult.ToArray();
            */
            throw new NotImplementedException();
        }
    }
}

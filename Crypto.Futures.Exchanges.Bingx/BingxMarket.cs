using Crypto.Futures.Exchanges.Bingx.Data;
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

        public BingxMarket( BingxFutures oExchange ) 
        { 
            m_oExchange = oExchange;
            Websocket = new BingxWebsocketPublic(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }
        public IWebsocketPublic Websocket { get; }


        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.ExchangeData.GetFundingRatesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;

            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach( var oFunding in oResult.Data )
            {
                if (oFunding == null) continue;
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oFunding.Symbol);
                if (oSymbol == null) continue; // Skip if symbol is not found
                aResult.Add(new BingxFundingRate(oSymbol, oFunding));
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
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.ExchangeData.GetTickersAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;
            List<ITicker> aResult = new List<ITicker>();
            foreach (var oTicker in oResult.Data)
            {
                if (oTicker == null) continue;
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oTicker.Symbol);
                if (oSymbol == null) continue; // Skip if symbol is not found
                if(aSymbols != null)
                {
                    if (!aSymbols.Any(p => p.Symbol == oSymbol.Symbol)) continue; // Filter by symbols
                }   
                aResult.Add(new BingxTicker(oSymbol, oTicker));
            }
            return aResult.ToArray();
        }
    }
    
}

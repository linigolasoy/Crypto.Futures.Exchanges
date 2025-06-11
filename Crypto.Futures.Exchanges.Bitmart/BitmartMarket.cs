using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    internal class BitmartMarket : IFuturesMarket
    {

        private BitmartFutures m_oExchange;
        public BitmartMarket( BitmartFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }
        public IWebsocketPublic Websocket { get => throw new NotImplementedException(); }

        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            var oResult = await m_oExchange.RestClient.DoGetArray<IFundingRate?>(BitmartFutures.ENDP_SYMBOLS, "symbols", p => m_oExchange.Parser.ParseFundingRate(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;
            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach (var item in oResult.Data)
            {
                if( item == null) continue; 
                aResult.Add(item);
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
            if (aSymbols == null) return aAllFunding;
            return aAllFunding.Where(f => aSymbols.Any(s => f.Symbol.Symbol == s.Symbol)).ToArray();
        }
        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols)
        {
            var oResult = await m_oExchange.RestClient.DoGetArray<ITicker?>(BitmartFutures.ENDP_SYMBOLS, "symbols", p => m_oExchange.Parser.ParseTicker(p));
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;
            List<ITicker> aResult = new List<ITicker>();
            foreach (var item in oResult.Data)
            {
                if (item == null) continue;
                if( aSymbols != null )
                {
                    if (!aSymbols.Any(p => p.Symbol == item.Symbol.Symbol)) continue;
                }
                aResult.Add(item);
            }
            return aResult.ToArray();
        }
    }
}

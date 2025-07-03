using Crypto.Futures.Exchanges.Coinex.Data;
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
            var oResult = await m_oExchange.RestClient.FuturesApi.ExchangeData.GetFundingRatesAsync();
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null || oResult.Data.Length <= 0) return null;
            List<IFundingRate> aFundingRates = new List<IFundingRate>();    
            foreach (var oFunding in oResult.Data)
            {
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oFunding.Symbol);
                if (oSymbol == null) continue;
                aFundingRates.Add(new CoinexFundingRate(oSymbol, oFunding));
            }
            return aFundingRates.ToArray();
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
            string[]? aSymbolString = null;
            if(aSymbols != null && aSymbols.Length > 0)
            {
                aSymbolString = aSymbols.Select(s => s.Symbol).ToArray();
            }   
            var oResult = await m_oExchange.RestClient.FuturesApi.ExchangeData.GetTickersAsync(aSymbolString);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.Length <= 0) return null;
            List<ITicker> aTickers = new List<ITicker>();
            foreach (var oItem in oResult.Data)
            {
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbol == null) continue;
                aTickers.Add(new CoinexTicker(oSymbol, oItem));
            }
            return aTickers.ToArray();
        }
    }
    
}

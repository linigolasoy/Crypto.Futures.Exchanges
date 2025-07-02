using Crypto.Futures.Exchanges.Bitmart.Data;
using Crypto.Futures.Exchanges.Bitmart.Ws;
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
            Websocket = new BitmartWebsocketPublic(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }
        public IWebsocketPublic Websocket { get; }

        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.ExchangeData.GetContractsAsync();
            if( oResult == null || !oResult.Success) return null;
            List<IFundingRate> aFundingRates = new List<IFundingRate>();
            foreach (var oItem in oResult.Data )
            {
                if(oItem == null) continue; // Skip null items
                IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbol == null) continue; // Skip symbols that are not managed by the exchange
                aFundingRates.Add(new BitmartFundingRate(oSymbol, oItem));
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
            if (aSymbols == null) return aAllFunding;
            return aAllFunding.Where(f => aSymbols.Any(s => f.Symbol.Symbol == s.Symbol)).ToArray();
        }
        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols)
        {
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.ExchangeData.GetContractsAsync();
            if (oResult == null || !oResult.Success) return null;
            List<ITicker> aResult = new List<ITicker>();
            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue; // Skip null items
                IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbol == null) continue; // Skip symbols that are not managed by the exchange
                if( oItem.LastPrice == null || oItem.LastPrice <= 0) continue; // Skip items with no price  
                if( aSymbols != null && !aSymbols.Any(s => s.Symbol == oSymbol.Symbol)) continue; // Skip symbols not in the requested list
                aResult.Add(new BitmartTicker(oSymbol, oItem));
            }

            return aResult.ToArray();
        }
    }
}

using Bitget.Net.Enums;
using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Bitget.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    /// <summary>
    /// Market data class for Bitget Futures Exchange.  
    /// </summary>
    internal class BitgetMarket: IFuturesMarket
    {

        private BitgetFutures m_oExchange;
        public BitgetMarket(BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
            Websocket = new BitgetWebsocketPublic(this);
        }

        public IFuturesExchange Exchange { get =>m_oExchange; }

        public IWebsocketPublic Websocket { get; }


        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            var oResult = await m_oExchange.RestClient.FuturesApiV2.ExchangeData.GetFundingRatesAsync(BitgetProductTypeV2.UsdtFutures);
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null || oResult.Data.Length <= 0) return null;
            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach ( var oData in oResult.Data )
            {
                if( oData == null) continue;    
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue; // Skip if symbol is not found
                aResult.Add(new BitgetFundingRate(oSymbol, oData));
            }
            return aResult.ToArray();   

        }
        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            IFundingRate[]? aAllFunding = await GetAllFundingRates();
            if (aAllFunding == null) return null;
            return aAllFunding.FirstOrDefault(f => f.Symbol.Symbol == oSymbol.Symbol);
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols = null)
        {
            IFundingRate[]? aAllFunding = await GetAllFundingRates();
            if (aAllFunding == null) return null;
            if (aSymbols == null) return aAllFunding;
            return aAllFunding.Where(f => aSymbols.Any(s => f.Symbol.Symbol == s.Symbol)).ToArray();
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols = null)
        {

            var oResult = await m_oExchange.RestClient.FuturesApiV2.ExchangeData.GetTickersAsync(BitgetProductTypeV2.UsdtFutures);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null || oResult.Data.Length <= 0) return null;
            List<ITicker> aResult = new List<ITicker>();
            foreach (var oData in oResult.Data)
            {
                if (oData == null) continue;
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue; // Skip if symbol is not found
                aResult.Add(new BitgetTicker(oSymbol, oData));
            }
            return aResult.ToArray();   
        }
    }
}

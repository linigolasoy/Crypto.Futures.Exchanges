using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Crypto.Futures.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Futures.Exchanges.Blofin.Data;
using Newtonsoft.Json;
using Cypto.Futures.Exchanges.Blofin.Data;
using Crypto.Futures.Exchanges.Rest;

namespace Cypto.Futures.Exchanges.Blofin
{
    internal class BlofinMarket : IFuturesMarket
    {

        private BlofinFutures m_oExchange;
        private const string ENDP_FUNDING = "/api/v1/market/funding-rate";

        public BlofinMarket(BlofinFutures oExchange)
        {
            m_oExchange = oExchange;
            // Websocket = new BlofinWebsocketPublic(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket { get=> throw new NotImplementedException(); }    


        /// <summary>
        /// Get all funding rates
        /// </summary>
        /// <returns></returns>
        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            try
            {
                BaseApiCaller oApiCalled = new BaseApiCaller(BlofinFutures.BASE_URL);
                var oCallResult = await oApiCalled.GetAsync(ENDP_FUNDING);

                if (oCallResult == null || !oCallResult.Success || oCallResult.Data== null) return null;
                BlofinResponse? oResponse = JsonConvert.DeserializeObject<BlofinResponse>(oCallResult.Data);
                if (oResponse == null || oResponse.code != 0 || oResponse.data == null) return null;
                return BlofinFundingRate.ParseAll(m_oExchange, oResponse.data);


            }
            catch( Exception ex )
            {
                if(m_oExchange.Logger != null ) m_oExchange.Logger.Error($"Error getting funding rates: {ex.Message}", ex);
                return null;
            }
        }


        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            IFundingRate[]? aAll = await GetAllFundingRates();
            if (aAll == null) return null;
            return aAll.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols = null)
        {
            IFundingRate[]? aAll = await GetAllFundingRates();
            if (aAll == null) return null;
            if (aSymbols == null) return aAll;
            return aAll.Where(p => aSymbols.Any(q => p.Symbol.Symbol == q.Symbol)).ToArray();
        }
        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols)
        {
            try
            {
                var oResult = await m_oExchange.RestClient.FuturesApi.ExchangeData.GetTickersAsync();
                if (oResult == null || !oResult.Success || oResult.Data == null) return null;
                List<ITicker> aReturn = new List<ITicker>();
                foreach (var oTicker in oResult.Data)
                {
                    IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oTicker.Symbol);
                    if (oSymbol == null) continue; // Skip unknown symbols
                    aReturn.Add(new BlofinTickerMine(oSymbol, oTicker));
                }
                return aReturn.ToArray();
            }
            catch( Exception ex )
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Error getting tickers: {ex.Message}", ex);
                return null;
            }
        }
    }
}

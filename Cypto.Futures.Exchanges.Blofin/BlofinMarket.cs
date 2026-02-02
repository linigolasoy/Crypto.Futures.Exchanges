using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Crypto.Futures.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crypto.Futures.Exchanges.Blofin.Ws;
using Crypto.Futures.Exchanges.Blofin.Data;
using Newtonsoft.Json;
using Cypto.Futures.Exchanges.Blofin.Data;

namespace Cypto.Futures.Exchanges.Blofin
{
    internal class BlofinMarket : IFuturesMarket
    {

        private BlofinFutures m_oExchange;
        private const string ENDP_FUNDING = "/api/v1/market/funding-rate";
        private const string ENDP_TICKER = "/api/v1/market/tickers";

        public BlofinMarket(BlofinFutures oExchange)
        {
            m_oExchange = oExchange;
            Websocket = new BlofinWebsocketPublic(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket { get; }


        /// <summary>
        /// Get all funding rates
        /// </summary>
        /// <returns></returns>
        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            
            var oResult = await m_oExchange.ApiCaller.GetAsync(ENDP_FUNDING);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            BlofinResponse? oResponse = JsonConvert.DeserializeObject<BlofinResponse>(oResult.Data);
            if (oResponse == null || !oResponse.IsSuccess() || oResponse.data == null) return null;
            return BlofinFundingRate.ParseAll(m_oExchange, oResponse.data);
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
            /*
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<ITicker?>(ENDP_TICKER, null, p => m_oExchange.Parser.ParseTicker(p));
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
            */
            throw new NotImplementedException();
        }
    }
}

using Crypto.Futures.Exchanges.Bitunix.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitunix
{
    internal class BitunixMarket : IFuturesMarket
    {
        private BitunixFutures m_oExchange;

        private const string ENDP_FUNDING = "/api/v1/futures/market/funding_rate/batch";
        private const string ENDP_TICKERS = "/api/v1/futures/market/tickers";

        public BitunixMarket(BitunixFutures oExchange)
        {
            m_oExchange = oExchange;
            // Websocket = new BitunixWebsocketPublic(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket => throw new NotImplementedException();


        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            
            var oResult = await m_oExchange.ApiCaller.GetAsync(ENDP_FUNDING);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            BitunixResponse? oResponse = JsonConvert.DeserializeObject<BitunixResponse>(oResult.Data);
            if (oResponse == null || !oResponse.IsSuccess() || oResponse.data == null) return null;

            return BitunixFundingRate.ParseAll(m_oExchange, oResponse.data);
        }
        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            IFundingRate[]? aRates = await GetFundingRates(new IFuturesSymbol[] { oSymbol });
            if (aRates == null) return null;
            return aRates.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol);
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols = null)
        {
            IFundingRate[]? aAll = await GetAllFundingRates();
            if (aAll == null) return null;
            if (aSymbols == null || aSymbols.Length <= 0) return aAll;
            return aAll.Where(p => aSymbols.Any(s => s.Symbol == p.Symbol.Symbol)).ToArray();
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols = null)
        {
            var oResult = await m_oExchange.ApiCaller.GetAsync(ENDP_TICKERS);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            BitunixResponse? oResponse = JsonConvert.DeserializeObject<BitunixResponse>(oResult.Data);
            if (oResponse == null || oResponse.data == null || !oResponse.IsSuccess()) return null;

            return BitunixTicker.ParseAll(this.Exchange, oResponse.data);
        }
    }
}

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
    internal class BitgetMarket : IFuturesMarket
    {
        private BitgetFutures m_oExchange;

        private const string ENDP_FUNDINGRATE = "/mix/market/current-fund-rate";
        private const string ENDP_TICKERS = "/mix/market/tickers";
        // curl "https://api.bitget.com/api/v2/mix/market/current-fund-rate?symbol=BTCUSDT&productType=usdt-futures"
        public BitgetMarket(BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
            Websocket = new BitgetWebsocketPublic(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }
        public IWebsocketPublic Websocket { get; }

        private async Task<IFundingRate[]?> GetAllFundingRates()
        {
            Dictionary<string, string> aParams = new Dictionary<string, string>();
            aParams.Add("productType", "usdt-futures");
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<IFundingRate?>(ENDP_FUNDINGRATE, null, p => m_oExchange.Parser.ParseFundingRate(p), aParams);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;
            List<IFundingRate> aResult = new List<IFundingRate>();
            foreach (var f in oResult.Data)
            {
                if (f != null) aResult.Add(f);
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
            Dictionary<string, string> aParams = new Dictionary<string, string>();
            aParams.Add("productType", "usdt-futures");
            var oResult = await m_oExchange.RestClient.DoGetArrayParams<ITicker?>(ENDP_TICKERS, null, p => m_oExchange.Parser.ParseTicker(p), aParams);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Count() <= 0) return null;
            List<ITicker> aResult = new List<ITicker>();
            foreach (var f in oResult.Data)
            {
                if (f == null) continue;
                if( aSymbols != null )
                {
                    if (!aSymbols.Any(p => p.Symbol == f.Symbol.Symbol)) continue;
                }
                aResult.Add(f);
            }

            return aResult.ToArray();
        }
    }
}

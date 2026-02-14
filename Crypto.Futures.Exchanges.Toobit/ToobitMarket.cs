using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Toobit.Data;
using Crypto.Futures.Exchanges.Toobit.Ws;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Toobit
{
    internal class ToobitMarket : IFuturesMarket
    {
        private ToobitFutures m_oExchange;

        public ToobitMarket(ToobitFutures oExchange)
        {
            m_oExchange = oExchange;
            Websocket = new ToobitWebsocketPublic(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket { get; }

        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            var aRates = await GetFundingRates(new IFuturesSymbol[] { oSymbol });
            if (aRates == null || aRates.Length <= 0) return null;
            return aRates[0];
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols = null)
        {
            try
            {
                var oRaw = await m_oExchange.RestClient.UsdtFuturesApi.ExchangeData.GetFundingRateAsync();
                if (oRaw == null || !oRaw.Success) return null;
                if( oRaw.Data == null || oRaw.Data.Length <= 0) return null;
                List<IFundingRate> aRates = new List<IFundingRate>();
                foreach (var oItem in oRaw.Data)
                {
                    IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);

                    if (oSymbol == null) continue; // Skip unknown symbols 
                    IFundingRate oRate = new ToobitFundingRateMine(oSymbol, oItem);
                    aRates.Add(oRate);
                }

                if( aSymbols == null || aSymbols.Length <= 0) return aRates.ToArray();

                return aRates.Where(p=> aSymbols.Any(q=> q.Symbol == p.Symbol.Symbol)).ToArray();
            }
            catch( Exception ex )
            {
                if(m_oExchange.Logger != null) m_oExchange.Logger.Error($"Error getting funding rates: {ex.Message}",ex);
                return null;
            }
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols = null)
        {
            try
            {
                var oRaw = await m_oExchange.RestClient.UsdtFuturesApi.ExchangeData.GetTickersAsync();
                if (oRaw == null || !oRaw.Success) return null;
                if (oRaw.Data == null || oRaw.Data.Length <= 0) return null;
                List<ITicker> aResult = new List<ITicker>();
                foreach (var oItem in oRaw.Data)
                {
                    IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);

                    if (oSymbol == null) continue; // Skip unknown symbols 
                    if( oItem.LastPrice == null || oItem.LastPrice <= 0) continue; // Skip invalid prices
                    aResult.Add(new ToobitTickerMine(oSymbol, oItem));
                    // IFundingRate oRate = new ToobitFundingRateMine(oSymbol, oItem);
                    // aRates.Add(oRate);
                }

                if (aSymbols == null || aSymbols.Length <= 0) return aResult.ToArray();

                return aResult.Where(p => aSymbols.Any(q => q.Symbol == p.Symbol.Symbol)).ToArray();
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Error getting tickers: {ex.Message}", ex);
                return null;
            }
        }
    }
}

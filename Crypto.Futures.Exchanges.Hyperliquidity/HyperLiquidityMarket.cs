using Crypto.Futures.Exchanges.Hyperliquidity.Data;
using Crypto.Futures.Exchanges.Hyperliquidity.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;

namespace Crypto.Futures.Exchanges.Hyperliquidity
{
    internal class HyperLiquidityMarket : IFuturesMarket
    {

        private HyperliquidityExchanges m_oExchange;
        public HyperLiquidityMarket(HyperliquidityExchanges oExchange)
        {
            m_oExchange = oExchange;
            Websocket = new HyperWebsocketPublic(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket { get; }

        public async Task<IFundingRate?> GetFundingRate(IFuturesSymbol oSymbol)
        {
            var oResult = await GetFundingRates(new IFuturesSymbol[] { oSymbol });
            if (oResult != null && oResult.Length > 0)
                return oResult[0];
            return null;
        }

        public async Task<IFundingRate[]?> GetFundingRates(IFuturesSymbol[]? aSymbols = null)
        {
            try
            {
                var aTickers = await m_oExchange.RestClient.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync();
                if (aTickers == null || !aTickers.Success || aTickers.Data == null) return null;
                if (aTickers.Data.ExchangeInfo.Symbols == null || aTickers.Data.ExchangeInfo.Symbols.Length == 0) return null;
                List<IFundingRate> aResult = new List<IFundingRate>();
                DateTime dNow = DateTime.Now;
                DateTime dNex = new DateTime(dNow.Year, dNow.Month, dNow.Day, dNow.Hour, 0, 0, DateTimeKind.Local).AddHours(1); // Funding every hour
                foreach (var sym in aTickers.Data.Tickers)
                {
                    IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(sym.Symbol);
                    if (oSymbol == null) continue;
                    if(sym.FundingRate == null) continue;
                    decimal nRate = sym.FundingRate.Value;


                    aResult.Add(new HyperFundingRate(oSymbol,dNex, nRate));

                }
                return aResult.ToArray();
            }
            catch (Exception ex)
            {
                if (Exchange.Logger != null) Exchange.Logger.Error("Error refreshing funding rates", ex);
                return null;
            }
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols = null)
        {
            try
            {
                var aTickers = await m_oExchange.RestClient.FuturesApi.ExchangeData.GetExchangeInfoAndTickersAsync();
                if (aTickers == null || !aTickers.Success || aTickers.Data == null) return null;
                if (aTickers.Data.ExchangeInfo.Symbols == null || aTickers.Data.ExchangeInfo.Symbols.Length == 0) return null;
                List<ITicker> aResult = new List<ITicker>();
                DateTime dNow = DateTime.Now;
                foreach (var sym in aTickers.Data.Tickers)
                {
                    IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(sym.Symbol);
                    if (oSymbol == null) continue;
                    decimal nPrice = sym.MarkPrice;
                    ITicker oTicker = new HyperTicker(oSymbol, dNow, nPrice);


                    aResult.Add(oTicker);

                }
                return aResult.ToArray();
            }
            catch (Exception ex)
            {
                if (Exchange.Logger != null) Exchange.Logger.Error("Error refreshing funding rates", ex);
                return null;
            }
        }
    }
}

using Crypto.Futures.Exchanges.Hyperliquidity.Parsing;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity
{
    internal class HyperLiquidityMarket : IFuturesMarket
    {

        private HyperliquidityExchanges m_oExchange;
        public HyperLiquidityMarket(HyperliquidityExchanges oExchange)
        {
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPublic Websocket => throw new NotImplementedException();

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
                PostInfoParams oParams = new PostInfoParams();
                var oResult = await m_oExchange.ApiCaller.PostAsync(HyperliquidityExchanges.ENDP_SYMBOLS, oParams);
                if (oResult == null || !oResult.Success || oResult.Data == null) return null;

                IFundingRate[] aResult = SymbolMetadataParser.ParseFunding(oResult.Data, this.Exchange);
                if( aSymbols != null )
                {
                    aResult = aResult.Where(p => aSymbols.Any(q=> p.Symbol.Symbol == q.Symbol)).ToArray();
                }
                return aResult;
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error("Error refreshing funding rates", ex);
                return null;
            }
        }

        public async Task<ITicker[]?> GetTickers(IFuturesSymbol[]? aSymbols = null)
        {
            throw new NotImplementedException();
        }
    }
}

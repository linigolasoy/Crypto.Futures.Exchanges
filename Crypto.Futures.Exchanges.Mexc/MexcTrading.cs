using Crypto.Futures.Exchanges.Mexc.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc
{

    /// <summary>
    /// Mexc trading implementation for futures trading actions.    
    /// </summary>
    internal class MexcTrading : IFuturesTrading
    {
        //private const string ENDP_ORDER = "/api/v1/private/order/submit";
        private const string ENDP_ORDER = "/api/v1/private/order/create";

        private MexcFutures m_oExchange;

        private MexcPrivate m_oExchangePrivate;
        public MexcTrading(MexcFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oExchangePrivate = new MexcPrivate(oExchange);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<bool> ClosePosition(IPosition oPosition, decimal? nPrice = null)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateOrder(IFuturesSymbol oSymbol, decimal nLeverage, bool bLong, decimal nQuantity, decimal? nPrice = null)
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;
                decimal nVolume = nQuantity / oSymbol.ContractSize;

                MexcOrderJson oPost = new MexcOrderJson()
                {
                    Symbol = oSymbol.Symbol,
                    Vol = nVolume,
                    Leverage = (int)nLeverage,
                    Side = (bLong ? (int)MexcOrderSide.OpenLong : (int)MexcOrderSide.OpenShort),
                    Price = ( nPrice == null ? 0 : nPrice.Value),
                    Type = (nPrice == null ? (int)MexcOrderType.Market : (int)MexcOrderType.Limit), // Market order
                    OpenType = (int)MexcOpenType.Cross // Isolated margin
                };


                var oResult = await oClient.DoPostParams<MexcOrderJson?>(ENDP_ORDER, oPost);
                if (oResult == null || !oResult.Success) return false;
                return oResult.Data;

            }
            catch (Exception ex)
            {
                if (Exchange.Logger != null)
                {
                    Exchange.Logger.Error("BloginAccount.GetBalances Error", ex);
                }
            }
            return false;
        }
    }
}

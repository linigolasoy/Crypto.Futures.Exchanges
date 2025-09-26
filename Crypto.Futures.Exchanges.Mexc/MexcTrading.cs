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

        public Task<bool> CloseOrders(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null, bool bFillOrKill = false)
        {
            throw new NotImplementedException();
        }

        public async Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null, bool bFillOrKill = false)
        {
            throw new NotImplementedException();    
        }
    }
}

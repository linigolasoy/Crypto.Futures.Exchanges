using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Cypto.Futures.Exchanges.Blofin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Blofin
{
    /// <summary>
    /// Account methods
    /// </summary>
    internal class BlofinAccount : IFuturesAccount
    {
        private BlofinFutures m_oExchange;

        private const string ENDP_BALANCES = "/api/v1/asset/balances";
        public BlofinAccount( BlofinFutures oExchange ) 
        { 
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IBalance[]?> GetBalances()
        {
            /*
            try
            {
                ICryptoRestClient oClient = m_oExchange.RestClient;
                oClient.OnRequestHeader += BlofinPrivate.PutHeaders;

                var oResult = await oClient.DoGetArray<IBalance?>(ENDP_BALANCES, null, p => m_oExchange.Parser.ParseBalance(p));
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;

            }
            catch ( Exception ex )
            { 
                if( Exchange.Logger != null )
                {
                    Exchange.Logger.Error("BloginAccount.GetBalances Error", ex);
                }
            }
            return null;
            */
            await Task.Delay(200);
            throw new NotImplementedException();
        }
        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            throw new NotImplementedException();
        }
        public async Task<IPosition[]?> GetPositions()
        {
            throw new NotImplementedException();
        }

        public async Task<IOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositionHistory(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }
    }
}

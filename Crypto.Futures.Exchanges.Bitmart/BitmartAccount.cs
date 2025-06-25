using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    internal class BitmartAccount : IFuturesAccount
    {
        private BitmartFutures m_oExchange;
        private BitmartPrivate m_oExchangePrivate;

        private const string ENDP_BALANCES = "/contract/private/assets-detail";
        public BitmartAccount(BitmartFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oExchangePrivate = new BitmartPrivate(oExchange);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IBalance[]?> GetBalances()
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;

                var oResult = await oClient.DoGetArrayParams<IBalance?>(ENDP_BALANCES, null, p => m_oExchange.Parser.ParseBalance(p));
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;
                List<IBalance> aResult = new List<IBalance>();
                foreach (var oItem in oResult.Data)
                {
                    if (oItem == null) continue;
                    aResult.Add(oItem);
                }
                return aResult.ToArray();
            }
            catch (Exception ex)
            {
                if (Exchange.Logger != null)
                {
                    Exchange.Logger.Error("BloginAccount.GetBalances Error", ex);
                }
            }
            return null;
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
    }
}

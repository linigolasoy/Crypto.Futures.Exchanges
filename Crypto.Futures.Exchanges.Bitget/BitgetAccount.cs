using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    internal class BitgetAccount : IFuturesAccount
    {
        private BitgetFutures m_oExchange;
        private BitgetPrivate m_oExchangePrivate;

        private const string ENDP_BALANCES = "/mix/account/accounts";
        public BitgetAccount( BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oExchangePrivate = new BitgetPrivate(oExchange);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IBalance[]?> GetBalances()
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;

                Dictionary<string,string> aDictParams = new Dictionary<string,string>();
                aDictParams.Add("productType", "USDT-FUTURES");
                var oResult = await oClient.DoGetArrayParams<IBalance?>(ENDP_BALANCES, null, p => m_oExchange.Parser.ParseBalance(p), aDictParams);
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
    }
}

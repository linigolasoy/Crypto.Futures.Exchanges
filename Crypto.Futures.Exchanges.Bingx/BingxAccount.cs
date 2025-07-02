using Crypto.Futures.Exchanges.Bingx.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx
{
    /*
    internal class BingxAccount : IFuturesAccount
    {
        private BingxFutures m_oExchange;
        private BingxPrivate m_oExchangePrivate;
        private const string ENDP_BALANCES = "/openApi/swap/v3/user/balance";
        private const string ENDP_LEVERAGE = "/openApi/swap/v2/trade/leverage";
        private const string ENDP_SETLEVERAGE = "/openApi/swap/v2/trade/leverage";
        private const string ENDP_POSITIONS = "/openApi/swap/v2/user/positions";
        public BingxAccount(BingxFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oExchangePrivate = new BingxPrivate(oExchange);
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

        /// <summary>
        /// Leverage get
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;
                Dictionary<string, string> aParams = new Dictionary<string, string>();
                aParams.Add("symbol", oSymbol.Symbol);  
                var oResult = await oClient.DoGetParams<BingxLeverage?>(ENDP_LEVERAGE, p => p.ToObject<BingxLeverage>(), aParams);
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                decimal nLeverage = oResult.Data.LongLeverage;
                if( oResult.Data.ShortLeverage < nLeverage)
                {
                    nLeverage = oResult.Data.ShortLeverage;
                }   
                return nLeverage;
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

        /// <summary>
        /// Set leverage for a symbol.  
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="nLeverage"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;

                BingxLeveragePost oPost = new BingxLeveragePost()
                {
                    Symbol = oSymbol.Symbol,
                    Leverage = (int)nLeverage,
                    Side = "BOTH" 
                };

                
                var oResult = await oClient.DoPostParams<BingxLeveragePost?>(ENDP_SETLEVERAGE, oPost);
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

        /// <summary>
        /// Get all positions for the account.  
        /// </summary>
        /// <returns></returns>
        public async Task<IPosition[]?> GetPositions()
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;

                var oResult = await oClient.DoGetArrayParams<IPosition?>(ENDP_POSITIONS, null, p => m_oExchange.Parser.ParsePosition(p));
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                List<IPosition> aResult = new List<IPosition>();
                if (oResult.Data.Count() <= 0) return aResult.ToArray();
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
    */
}

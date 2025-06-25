using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Concurrent;
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
        private const string ENDP_SETLEVERAGE = "/mix/account/set-leverage"; // Endpoint to set leverage
        private const decimal DEFAULT_LEVERAGE = 2m; // Default leverage value, can be adjusted as needed   
        private const string ENDP_POSITIONS = "/mix/position/all-position";

        private static ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();
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

        /// <summary>
        /// Get the leverage for a specific futures symbol. 
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            decimal nLeverage = DEFAULT_LEVERAGE;
            if (m_aLeverages.TryGetValue(oSymbol.Symbol, out nLeverage))
            {
                return nLeverage;
            }
            m_aLeverages.TryAdd(oSymbol.Symbol, DEFAULT_LEVERAGE);
            await Task.Delay(100); // Simulate network delay
            return DEFAULT_LEVERAGE;
        }

        /// <summary>
        /// Set the leverage for a specific futures symbol. 
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="nLeverage"></param>
        /// <returns></returns>
        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;

                BitgetLeveragePost oPost = new BitgetLeveragePost()
                {
                    Symbol = oSymbol.Symbol,
                    MarginCoin = "USDT",                    
                    Leverage = ((int)nLeverage).ToString()
                };


                var oResult = await oClient.DoPostParams<BitgetLeveragePost?>(ENDP_SETLEVERAGE, oPost);
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
        /// Get the positions for the account.
        /// </summary>
        /// <returns></returns>
        public async Task<IPosition[]?> GetPositions()
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;

                Dictionary<string, string> aDictParams = new Dictionary<string, string>();
                aDictParams.Add("productType", "USDT-FUTURES");
                var oResult = await oClient.DoGetArrayParams<IPosition?>(ENDP_POSITIONS, null, p => m_oExchange.Parser.ParsePosition(p), aDictParams);
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
}

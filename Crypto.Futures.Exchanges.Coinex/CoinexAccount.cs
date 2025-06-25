using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex
{
    internal class CoinexAccount : IFuturesAccount
    {

        private const string ENDP_BALANCES = "/assets/futures/balance";
        private const string ENDP_SETLEVERAGE = "/futures/adjust-position-leverage";
        private const string ENDP_POSITIONS = "/futures/pending-position";
        private CoinexFutures m_oExchange;
        private CoinexPrivate m_oExchangePrivate;

        private const decimal DEFAULT_LEVERAGE = 2m; // Default leverage value, can be adjusted as needed   

        private static ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();    
        public CoinexAccount( CoinexFutures oExchange ) 
        { 
            m_oExchange = oExchange;
            m_oExchangePrivate = new CoinexPrivate(oExchange);
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
        /// Returns leverages
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            decimal nLeverage = DEFAULT_LEVERAGE;
            if( m_aLeverages.TryGetValue(oSymbol.Symbol, out nLeverage))
            {
                return nLeverage;
            }
            m_aLeverages.TryAdd(oSymbol.Symbol, DEFAULT_LEVERAGE);
            await Task.Delay(100); // Simulate network delay
            return DEFAULT_LEVERAGE;
        }

        /// <summary>
        /// Sets the leverage for a given symbol.   
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

                CoinexLeveragePost oPost = new CoinexLeveragePost()
                {
                    Market = oSymbol.Symbol,
                    MarketType = "FUTURES",
                    MarginMode = "cross", 
                    Leverage = (int)nLeverage
                };


                var oResult = await oClient.DoPostParams<CoinexLeveragePost?>(ENDP_SETLEVERAGE, oPost);
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
        /// Get poisitions for the account.
        /// </summary>
        /// <returns></returns>
        public async Task<IPosition[]?> GetPositions()
        {
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;
                Dictionary<string, string> aParams = new Dictionary<string, string>();
                aParams.Add("market_type", "FUTURES");
                var oResult = await oClient.DoGetArrayParams<IPosition?>(ENDP_POSITIONS, null, p => m_oExchange.Parser.ParsePosition(p), aParams);
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

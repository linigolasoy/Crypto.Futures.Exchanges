using Crypto.Futures.Exchanges.Mexc.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc
{
    internal class MexcAccount : IFuturesAccount
    {
        private MexcFutures m_oExchange;

        private MexcPrivate m_oExchangePrivate;

        private const string ENDP_BALANCES = "/api/v1/private/account/assets";
        private const string ENDP_LEVERAGE = "/api/v1/private/position/leverage";
        private const string ENDP_SETLEVERAGE = "/api/v1/private/position/change_leverage";
        private const string ENDP_POSITIONS = "/api/v1/private/position/open_positions";
        public MexcAccount(MexcFutures oExchange)
        {
            m_oExchange = oExchange;
            m_oExchangePrivate = new MexcPrivate(oExchange);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPrivate WebsocketPrivate => throw new NotImplementedException();

        /*
ApiKey, Request-Time, Signature
        */
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
                    if( oItem == null) continue;
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
            try
            {
                CryptoRestClient oClient = m_oExchange.RestClient;

                oClient.RequestEvaluator = m_oExchangePrivate.CreatePrivateRequest;

                Dictionary<string, string> aParameters = new Dictionary<string, string>();  
                aParameters.Add("symbol", oSymbol.Symbol);
                var oResult = await oClient.DoGetArrayParams<MexcLeverage?>(ENDP_LEVERAGE, null, p => p.ToObject<MexcLeverage>(), aParameters);
                if (oResult == null || !oResult.Success) return null;
                if (oResult.Data == null) return null;
                if (oResult.Data.Count() <= 0) return null;
                List<MexcLeverage> aResult = new List<MexcLeverage>();
                foreach (var oItem in oResult.Data)
                {
                    if (oItem == null) continue;
                    aResult.Add(oItem);
                }
                decimal nMin = aResult.Min(p => p.Leverage);
                return nMin;
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
        /// Sets leverage
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

                MexcLeveragePost oPostLong = new MexcLeveragePost()
                {
                    Symbol = oSymbol.Symbol,
                    Leverage = (int)nLeverage,
                    PositionType = 1, // 1 = Long
                    OpenType = 1
                };

                MexcLeveragePost oPostShort = new MexcLeveragePost()
                {
                    Symbol = oSymbol.Symbol,
                    Leverage = (int)nLeverage,
                    PositionType = 2, // 2 = Long
                    OpenType = 1
                };

                var oResult = await oClient.DoPostParams<MexcLeveragePost?>(ENDP_SETLEVERAGE, oPostLong);
                if (oResult == null || !oResult.Success) return false;

                oResult = await oClient.DoPostParams<MexcLeveragePost?>(ENDP_SETLEVERAGE, oPostShort);
                if (oResult == null || !oResult.Success) return false;

                return true;
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
        /// Get all open positions for the account. 
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
                if (oResult.Data.Count() <= 0) aResult.ToArray();
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

        public async Task<IOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositionHistory(IFuturesSymbol oSymbol   )
        {
            throw new NotImplementedException();
        }
    }
}

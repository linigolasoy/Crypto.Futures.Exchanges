using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Toobit.Data;
using Crypto.Futures.Exchanges.Toobit.Ws;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Toobit
{
    /// <summary>
    /// Toobit account implementation
    /// </summary>
    internal class ToobitAccount : IFuturesAccount
    {

        private ToobitFutures m_oExchange;
        private ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();
        public ToobitAccount(ToobitFutures oExchange)
        {
            m_oExchange = oExchange;
            WebsocketPrivate = new ToobitWebsocketPrivate(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPrivate WebsocketPrivate { get; }

        public async Task<IBalance[]?> GetBalances()
        {

            try
            {
                var oBalances = await m_oExchange.RestClient.UsdtFuturesApi.Account.GetBalancesAsync();
                if( oBalances == null || !oBalances.Success || oBalances.Data == null) return null;
                List<IBalance> aResult = new List<IBalance>();
                foreach (var oData in oBalances.Data)
                {
                    if( oData == null ) continue;   
                    aResult.Add(new ToobitBalance(this.Exchange, oData));    
                }
                return aResult.ToArray();
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error("Error getting balances", ex);
                return null;
            }

        }

        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            try
            {
                if( m_aLeverages.TryGetValue(oSymbol.Symbol, out var nLeverage) ) return nLeverage;
                var oLeverage = await m_oExchange.RestClient.UsdtFuturesApi.Account.GetLeverageInfoAsync(oSymbol.Symbol);
                if (oLeverage == null || !oLeverage.Success || oLeverage.Data == null) return null;

                m_aLeverages[oSymbol.Symbol] = oLeverage.Data.Leverage;
                return oLeverage.Data.Leverage;
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error("Error getting leverage", ex);
                return null;
            }
        }

        public async Task<IOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositionHistory(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositions()
        {
            try
            {
                var aResult = await m_oExchange.RestClient.UsdtFuturesApi.Trading.GetPositionsAsync();
                if( aResult == null || !aResult.Success ) return null;
                List<IPosition> aPositions = new List<IPosition>();
                if (aResult.Data != null)
                {
                    foreach (var oData in aResult.Data)
                    {
                        IFuturesSymbol? oSymbol = Exchange.SymbolManager.GetSymbol(oData.Symbol);
                        if (oSymbol == null) continue;
                        aPositions.Add(new ToobitPositionMine(oSymbol, oData));
                    }
                }
                return aPositions.ToArray();

            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error("Error getting positions", ex);
                return null;
            }
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            try
            {
                if (m_aLeverages.TryGetValue(oSymbol.Symbol, out var nOldLeverage))
                {
                    if( nOldLeverage == nLeverage ) return true;
                }
                var oLeverage = await m_oExchange.RestClient.UsdtFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol,(int)nLeverage);
                if (oLeverage == null || !oLeverage.Success || oLeverage.Data == null) return false;

                m_aLeverages[oSymbol.Symbol] = oLeverage.Data.Leverage;
                return true;    
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error("Error setting leverage", ex);
                return false;
            }
        }
    }
}

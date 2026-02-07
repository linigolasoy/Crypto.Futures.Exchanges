using Crypto.Futures.Exchanges.Hyperliquidity.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Interfaces.Clients;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity
{
    internal class HyperLiquidityAccount : IFuturesAccount
    {
        private HyperliquidityExchanges m_oExchange;
        private ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();   
        public HyperLiquidityAccount(HyperliquidityExchanges oExchange)
        {
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPrivate WebsocketPrivate => throw new NotImplementedException();

        public async Task<IBalance[]?> GetBalances()
        {
            try
            {
                var oAccountInfo = await m_oExchange.RestClient.FuturesApi.Account.GetAccountInfoAsync(Exchange.ApiKey.ApiKey);
                if (!oAccountInfo.Success || oAccountInfo.Data == null)
                {
                    if (m_oExchange.Logger != null) m_oExchange.Logger?.Error($"HyperLiquidityAccount.GetBalances: Failed to get account info: {oAccountInfo.Error}");
                    return null;
                }

                decimal nAvaliable = oAccountInfo.Data.Withdrawable;
                decimal nUsed = oAccountInfo.Data.CrossMaintenanceMarginUsed;
                IBalance oBalance = new HyperBalance(Exchange, "USDT", nAvaliable, nUsed);
                return new IBalance[] { oBalance };
            }
            catch (Exception ex)
            {
                if( m_oExchange.Logger != null ) m_oExchange.Logger?.Error($"HyperLiquidityAccount.GetBalances: {ex.Message}");
            }
            return null;
        }

        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            if (m_aLeverages.TryGetValue(oSymbol.Symbol, out decimal nCurrentLeverage) )
            {
                return nCurrentLeverage;
            }
            return 1;
        }

        public async Task<IOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositionHistory(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// GetPositions retrieves the current open positions for the account. 
        /// </summary>
        /// <returns></returns>
        public async Task<IPosition[]?> GetPositions()
        {
            try
            {
                var oAccountInfo = await m_oExchange.RestClient.FuturesApi.Account.GetAccountInfoAsync(Exchange.ApiKey.ApiKey);
                if (!oAccountInfo.Success || oAccountInfo.Data == null)
                {
                    if (m_oExchange.Logger != null) m_oExchange.Logger?.Error($"HyperLiquidityAccount.GetPositions: Failed to get account info: {oAccountInfo.Error}");
                    return null;
                }
                if( oAccountInfo.Data.Positions == null || oAccountInfo.Data.Positions.Length <= 0)
                {
                    return Array.Empty<IPosition>();    
                }
                List<IPosition> aResult = new List<IPosition>();
                foreach (var pos in oAccountInfo.Data.Positions)
                {
                    IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(pos.Position.Symbol);
                    if (oSymbol == null) continue;
                    aResult.Add(new HyperPosition(oSymbol, pos));
                    // m_aLeverages.AddOrUpdate(pos.Symbol, pos.Leverage, (k, v) => pos.Leverage);
                }
                return aResult.ToArray();
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger?.Error($"HyperLiquidityAccount.GetPositions: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// SetLeverage sets the leverage for a given symbol. It checks the current leverage and only updates if it's different from the desired leverage.
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="nLeverage"></param>
        /// <returns></returns>
        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            if( m_aLeverages.TryGetValue(oSymbol.Symbol, out decimal nCurrentLeverage) && nCurrentLeverage == nLeverage)
            {
                return true;
            }
            var oAccountInfo = await m_oExchange.RestClient.FuturesApi.Trading.SetLeverageAsync(
                oSymbol.Symbol, 
                (int)nLeverage,
                HyperLiquid.Net.Enums.MarginType.Cross);

            if (oAccountInfo == null || !oAccountInfo.Success ) return false;
            m_aLeverages.AddOrUpdate(oSymbol.Symbol, nLeverage, (k, v) => nLeverage);
            return true;
        }
    }
}

using CoinEx.Net.Enums;
using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Coinex.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex
{
    /// <summary>
    /// Coinex account implementation for futures trading.
    /// </summary>
    internal class CoinexAccount : IFuturesAccount
    {
        private CoinexFutures m_oExchange;


        private static ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();
        public CoinexAccount(CoinexFutures oExchange)
        {
            m_oExchange = oExchange;
            WebsocketPrivate = new CoinexWebsocketPrivate(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPrivate WebsocketPrivate { get; }

        /// <summary>
        /// Account balances retrieval. 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IBalance[]?> GetBalances()
        {
            var oResult = await m_oExchange.RestClient.FuturesApi.Account.GetBalancesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IBalance> aResult = new List<IBalance>();
            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue;
                IBalance oBalance = new CoinexBalance(Exchange, oItem);
                if (oBalance.Balance + oBalance.Avaliable + oBalance.Locked <= 0) continue;
                aResult.Add(oBalance);
            }
            return aResult.ToArray();   
        }

        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            decimal nLeverage = 0;
            if (m_aLeverages.TryGetValue(oSymbol.Symbol, out nLeverage))
            {
                return nLeverage;
            }
            await Task.Delay(100);
            return 1;
        }

        public async Task<IOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositionHistory(IFuturesSymbol oSymbol   )
        {
            var oResult = await m_oExchange.RestClient.FuturesApi.Trading.GetPositionHistoryAsync(oSymbol.Symbol);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oItem in oResult.Data.Items)
            {
                if (oItem == null) continue;
                IFuturesSymbol? oSymbolFound = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbolFound == null) continue; // Skip if symbol is not found
                IPosition oPosition = new CoinexPosition(oSymbolFound, oItem);
                if (oPosition.Quantity <= 0) continue; // Only include positions with a quantity
                aResult.Add(oPosition);
            }
            return aResult.ToArray();   
        }

        /// <summary>
        /// Gets the current positions for the account. 
        /// </summary>
        /// <returns></returns>
        public async Task<IPosition[]?> GetPositions()
        {
            var oResult = await m_oExchange.RestClient.FuturesApi.Trading.GetPositionsAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oItem in oResult.Data.Items)
            {
                if (oItem == null) continue;
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbol == null) continue; // Skip if symbol is not found
                IPosition oPosition = new CoinexPosition(oSymbol, oItem);
                if (oPosition.Quantity <= 0) continue; // Only include positions with a quantity
                aResult.Add(oPosition);
            }
            return aResult.ToArray();
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            var oResult = await m_oExchange.RestClient.FuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, MarginMode.Cross, (int)nLeverage);
            if (oResult == null) return false;
            if (!oResult.Success)
            {
                if( Exchange.Logger != null)
                {
                    Exchange.Logger.Error($"Error setting leverage for {oSymbol.Symbol} to {nLeverage} [{CoinexFutures.GetErrorMessage(oResult)}]");
                }
                return false;
            }
            m_aLeverages[oSymbol.Symbol] = nLeverage;
            return true;
        }
    }
}

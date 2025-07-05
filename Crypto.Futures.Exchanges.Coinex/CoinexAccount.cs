using CoinEx.Net.Enums;
using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
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
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

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

        public async Task<IPosition[]?> GetPositionHistory()
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositions()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            var oResult = await m_oExchange.RestClient.FuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, MarginMode.Isolated, (int)nLeverage);
            if (oResult == null || !oResult.Success) return false;
            m_aLeverages[oSymbol.Symbol] = nLeverage;
            return true;
        }
    }
}

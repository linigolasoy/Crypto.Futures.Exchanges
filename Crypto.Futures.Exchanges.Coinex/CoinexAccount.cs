using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
using System;
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}

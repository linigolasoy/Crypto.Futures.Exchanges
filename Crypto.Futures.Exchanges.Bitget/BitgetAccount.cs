using Bitget.Net.Enums;
using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
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
        public BitgetAccount(BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        /// <summary>
        /// Get the balances for the account.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IBalance[]?> GetBalances()
        {
            var oResult = await m_oExchange.RestClient.FuturesApiV2.Account.GetBalancesAsync(BitgetProductTypeV2.UsdtFutures);
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null ) return null;
            List<IBalance> aResult = new List<IBalance>();
            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue;
                IBalance oBalance = new BitgetBalance(Exchange, oItem);
                if( oBalance.Balance + oBalance.Avaliable + oBalance.Locked <= 0) continue; // Ignore empty balances
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

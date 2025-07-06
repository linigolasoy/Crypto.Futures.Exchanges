using BingX.Net.Enums;
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
    
    internal class BingxAccount : IFuturesAccount
    {
        private BingxFutures m_oExchange;

        public BingxAccount(BingxFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IBalance[]?> GetBalances()
        {
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.Account.GetBalancesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null ) return null;
            List<IBalance> aResult = new List<IBalance>();  


            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue;
                IBalance oBalance = new BingxBalance(Exchange, oItem);
                if (oBalance.Balance + oBalance.Avaliable + oBalance.Locked  <= 0) continue;
                aResult.Add(oBalance);
            }
            return aResult.ToArray();
        }

        /// <summary>
        /// Leverage get
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<decimal?> GetLeverage(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.Account.GetLeverageAsync(oSymbol.Symbol);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.ShortLeverage != oResult.Data.LongLeverage) return -1;
            return oResult.Data.ShortLeverage; // Both should be the same, so return one of them    
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
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, PositionSide.Both, (int)nLeverage);
            if (oResult == null || !oResult.Success) return false;
            return true;
        }

        /// <summary>
        /// Get all positions for the account.  
        /// </summary>
        /// <returns></returns>
        public async Task<IPosition[]?> GetPositions()
        {
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.Trading.GetPositionsAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue;
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbol == null) continue; // Skip if symbol is not found
                IPosition oPosition = new BingxPosition(oSymbol, oItem);
                if (oPosition.Quantity <= 0) continue; // Only include positions with non-zero quantity
                aResult.Add(oPosition);
            }
            return aResult.ToArray();
        }

        public async Task<IOrder[]?> GetOrders()
        {
            throw new NotImplementedException();
        }

        public async Task<IPosition[]?> GetPositionHistory()
        {
            throw new NotImplementedException();
        }
    }
}

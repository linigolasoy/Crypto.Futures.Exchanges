using BitMart.Net.Enums;
using Crypto.Futures.Exchanges.Bitmart.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    
    internal class BitmartAccount : IFuturesAccount
    {
        private BitmartFutures m_oExchange;
        private static ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();
        public BitmartAccount(BitmartFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<IBalance[]?> GetBalances()
        {
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Account.GetBalancesAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IBalance> aResult = new List<IBalance>();
            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue;
                IBalance oBalance = new BitmartBalance(Exchange, oItem);
                if (oBalance.Balance + oBalance.Avaliable + oBalance.Locked <= 0) continue; // Ignore empty balances
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

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, (int)nLeverage, MarginType.IsolatedMargin );
            if (oResult == null || !oResult.Success) return false;
            m_aLeverages[oSymbol.Symbol] = nLeverage;
            return true;
        }
        public async Task<IPosition[]?> GetPositions()
        {
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Trading.GetPositionsAsync();
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue;
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbol == null) continue; // Ignore unknown symbols
                IPosition oPosition = new BitmartPositionMine(oSymbol, oItem);
                if (oPosition.Quantity <= 0) continue; // Ignore empty positions
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

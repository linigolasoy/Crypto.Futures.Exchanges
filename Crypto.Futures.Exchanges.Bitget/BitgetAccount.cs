using Bitget.Net.Enums;
using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Bitget.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    internal class BitgetAccount : IFuturesAccount
    {

        private BitgetFutures m_oExchange;
        private static ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();
        public BitgetAccount(BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
            WebsocketPrivate = new BitgetWebsocketPrivate(this);
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPrivate WebsocketPrivate { get; }

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
                IBalance oBalance = new BitgetBalanceMine(Exchange, oItem);
                if( oBalance.Balance + oBalance.Avaliable + oBalance.Locked <= 0) continue; // Ignore empty balances
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

        public async Task<IPosition[]?> GetPositionHistory(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oExchange.RestClient.FuturesApiV2.Trading.GetPositionHistoryAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oItem in oResult.Data.Entries)
            {
                if (oItem == null) continue;
                IFuturesSymbol? oSymbolFound = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbolFound == null) continue; // Ignore symbols that are not in the symbol manager
                IPosition oPosition = new BitgetPositionMine(oSymbolFound, oItem);
                if (oPosition.Quantity == 0) continue; // Ignore empty positions
                aResult.Add(oPosition);
            }
            return aResult.ToArray();
        }

        public async Task<IPosition[]?> GetPositions()
        {
            var oResult = await m_oExchange.RestClient.FuturesApiV2.Trading.GetPositionsAsync(BitgetProductTypeV2.UsdtFutures, "USDT");
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oItem in oResult.Data)
            {
                if (oItem == null) continue;
                IFuturesSymbol? oSymbol = m_oExchange.SymbolManager.GetSymbol(oItem.Symbol);
                if (oSymbol == null) continue; // Ignore symbols that are not in the symbol manager
                IPosition oPosition = new BitgetPositionMine(oSymbol, oItem);
                if (oPosition.Quantity == 0) continue; // Ignore empty positions
                aResult.Add(oPosition);
            }
            return aResult.ToArray();   
        }

        public async Task<bool> SetLeverage(IFuturesSymbol oSymbol, decimal nLeverage)
        {
            var oResult = await m_oExchange.RestClient.FuturesApiV2.Account.SetLeverageAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, "USDT", (int)nLeverage);
            if (oResult == null || !oResult.Success) return false;
            m_aLeverages[oSymbol.Symbol] = nLeverage;
            return true;
        }
    }
}

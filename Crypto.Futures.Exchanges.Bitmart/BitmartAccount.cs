using BitMart.Net.Enums;
using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Bitmart.Data;
using Crypto.Futures.Exchanges.Bitmart.Ws;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Rest;
using Crypto.Futures.Exchanges.WebsocketModel;
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
            WebsocketPrivate = new BitmartWebsocketPrivate(this);
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public IWebsocketPrivate WebsocketPrivate { get; }

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
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Account.SetLeverageAsync(oSymbol.Symbol, (int)nLeverage, MarginType.CrossMargin );
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

        public async Task<IPosition[]?> GetPositionHistory(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Trading.GetUserTradesAsync(oSymbol.Symbol);
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oItemOpen in oResult.Data.Where(p=> p.Side == FuturesSide.SellOpenShort || p.Side == FuturesSide.BuyOpenLong).OrderBy(p => p.CreateTime) )
            {
                if (oItemOpen == null) continue;
                FuturesSide eSideClose = (oItemOpen.Side == FuturesSide.BuyOpenLong ? FuturesSide.SellCloseLong : FuturesSide.BuyCloseShort);

                BitMartFuturesUserTrade? oItemClose = oResult.Data
                        .Where(p => p.Side == eSideClose && p.Symbol == oItemOpen.Symbol && p.CreateTime > oItemOpen.CreateTime && p.Quantity == oItemOpen.Quantity)
                        .OrderBy(p=> p.CreateTime).FirstOrDefault();
                if( oItemClose == null) continue; // No close trade found for this open trade   
                IPosition oPosition = new BitmartPositionMine(oSymbol, oItemOpen, oItemClose);
                aResult.Add(oPosition);
            }
            return aResult.ToArray();   
        }
    }
    
}

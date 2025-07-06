using CoinEx.Net.Enums;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex
{
    internal class CoinexTrading: IFuturesTrading
    {
        private readonly CoinexFutures m_oExchange;
        public CoinexTrading(CoinexFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<bool> CloseOrders(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ClosePosition(IPosition oPosition, decimal? nPrice = null)
        {
            OrderSide eSide = (oPosition.IsLong ? OrderSide.Sell : OrderSide.Buy);
            OrderTypeV2 eType = (nPrice == null ? OrderTypeV2.Market : OrderTypeV2.Limit);
            var oResult = await m_oExchange.RestClient.FuturesApi.Trading.PlaceOrderAsync(
                    oPosition.Symbol.Symbol, // string symbol, 
                    eSide, // OrderSide side, 
                    eType, // OrderTypeV2 type, 
                    oPosition.Quantity, // decimal quantity, 
                    nPrice //  decimal ? price = null, 
                           // string ? clientOrderId = null, 
                           // bool ? hide = null, 
                           // SelfTradePreventionMode ? stpMode = null, 
                           // CancellationToken ct = default(CancellationToken)
                );
            if (oResult == null || !oResult.Success) return false;
            if (oResult.Data == null) return false;
            if (oResult.Data.Id <= 0) return false;
            return true;
        }

        public async Task<bool> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null)
        {
            OrderSide eSide = (bLong ? OrderSide.Buy : OrderSide.Sell);
            OrderTypeV2 eType = (nPrice == null ? OrderTypeV2.Market : OrderTypeV2.Limit);
            var oResult = await m_oExchange.RestClient.FuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol, // string symbol, 
                    eSide, // OrderSide side, 
                    eType, // OrderTypeV2 type, 
                    nQuantity, // decimal quantity, 
                    nPrice //  decimal ? price = null, 
                           // string ? clientOrderId = null, 
                           // bool ? hide = null, 
                           // SelfTradePreventionMode ? stpMode = null, 
                           // CancellationToken ct = default(CancellationToken)
                );
            if (oResult == null || !oResult.Success) return false;
            if (oResult.Data == null) return false;
            if (oResult.Data.Id <= 0) return false;
            return true;
        }
    }
}

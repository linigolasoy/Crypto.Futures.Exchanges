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

        /// <summary>
        /// Close existing orders for the symbol.
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CloseOrders(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oExchange.RestClient.FuturesApi.Trading.GetOpenOrdersAsync(
                oSymbol.Symbol // string symbol, 
                               // CancellationToken ct = default(CancellationToken)
            );
            if (oResult == null || !oResult.Success) return true;
            if (oResult.Data == null || oResult.Data.Items.Length <= 0) return true;
            foreach (var oOrder in oResult.Data.Items)
            {
                var oCancelResult = await m_oExchange.RestClient.FuturesApi.Trading.CancelOrderAsync(
                    oSymbol.Symbol, // string symbol, 
                    oOrder.Id // long orderId, 
                              // CancellationToken ct = default(CancellationToken)
                );
                if (oCancelResult == null || !oCancelResult.Success) return false;
            }

            return true;
        }

        public async Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null)
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
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Id <= 0) return null;
            return oResult.Data.Id.ToString();
        }

        public async Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null)
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
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.Id <= 0) return null;
            return oResult.Data.Id.ToString();
        }
    }
}

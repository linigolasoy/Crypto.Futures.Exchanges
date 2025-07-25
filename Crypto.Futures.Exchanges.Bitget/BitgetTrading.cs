﻿using Bitget.Net.Enums.V2;
using Bitget.Net.Enums;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget
{
    /// <summary>
    /// BitgetTrading class implements the IFuturesTrading interface for Bitget Futures exchange.
    /// </summary>
    internal class BitgetTrading : IFuturesTrading
    {
        private readonly BitgetFutures m_oExchange;
        public BitgetTrading(BitgetFutures oExchange)
        {
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        /// <summary>
        /// Close existing orders for the symbol.
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<bool> CloseOrders(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oExchange.RestClient.FuturesApiV2.Trading.GetOpenOrdersAsync(
                BitgetProductTypeV2.UsdtFutures, // BitgetProductTypeV2 productType, 
                oSymbol.Symbol // string symbol, 
                               // CancellationToken ct = default(CancellationToken)
            );
            if (oResult == null || !oResult.Success) return false;
            if (oResult.Data == null || oResult.Data.Orders.Length <= 0) return true; // No open orders to close
            foreach (var oOrder in oResult.Data.Orders)
            {
                var oCancelResult = await m_oExchange.RestClient.FuturesApiV2.Trading.CancelOrderAsync(
                    BitgetProductTypeV2.UsdtFutures, // BitgetProductTypeV2 productType, 
                    oSymbol.Symbol, // string symbol, 
                    oOrder.OrderId // string orderId, 
                                   // CancellationToken ct = default(CancellationToken)
                );
                if (oCancelResult == null || !oCancelResult.Success) return false;
            }
            return true;
        }

        public async Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null)
        {
            OrderSide eSide = (!oPosition.IsLong) ? OrderSide.Sell : OrderSide.Buy;
            OrderType eType = (nPrice == null ? OrderType.Market : OrderType.Limit);
            TradeSide eTradeSide = TradeSide.Close; // (bLong ? TradeSide.Open)

            var oResult = await m_oExchange.RestClient.FuturesApiV2.Trading.PlaceOrderAsync(
                    BitgetProductTypeV2.UsdtFutures, // BitgetProductTypeV2 productType, 
                    oPosition.Symbol.Symbol, // string symbol, 
                    "USDT", // string marginAsset, 
                    eSide, // OrderSide side, 
                    eType, // OrderType type, 
                    MarginMode.CrossMargin, //  MarginMode marginMode, 
                    oPosition.Quantity, // decimal quantity, 
                    nPrice, //  decimal ? price = null, 
                    TimeInForce.GoodTillCanceled, // TimeInForce ? timeInForce = null, 
                    eTradeSide //  TradeSide. TradeSide ? tradeSide = null, 
                               // string ? clientOrderId = null, bool ? reduceOnly = null, decimal ? takeProfitPrice = null, decimal ? stopLossPrice = null, decimal ? takeProfitLimitPrice = null, decimal ? stopLossLimitPrice = null, CancellationToken ct = default(CancellationToken)
                );

            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (string.IsNullOrEmpty(oResult.Data.OrderId)) return null;
            return oResult.Data.OrderId;
        }

        public async Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null)
        {

            OrderSide eSide = (bLong) ? OrderSide.Buy : OrderSide.Sell;
            OrderType eType = (nPrice == null ? OrderType.Market : OrderType.Limit);
            TradeSide eTradeSide = TradeSide.Open; // (bLong ? TradeSide.Open)

            var oResult = await m_oExchange.RestClient.FuturesApiV2.Trading.PlaceOrderAsync(
                    BitgetProductTypeV2.UsdtFutures, // BitgetProductTypeV2 productType, 
                    oSymbol.Symbol, // string symbol, 
                    "USDT", // string marginAsset, 
                    eSide, // OrderSide side, 
                    eType, // OrderType type, 
                    MarginMode.CrossMargin, //  MarginMode marginMode, 
                    nQuantity, // decimal quantity, 
                    nPrice, //  decimal ? price = null, 
                    TimeInForce.GoodTillCanceled, // TimeInForce ? timeInForce = null, 
                    eTradeSide //  TradeSide. TradeSide ? tradeSide = null, 
                    // string ? clientOrderId = null, bool ? reduceOnly = null, decimal ? takeProfitPrice = null, decimal ? stopLossPrice = null, decimal ? takeProfitLimitPrice = null, decimal ? stopLossLimitPrice = null, CancellationToken ct = default(CancellationToken)
                );

            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (string.IsNullOrEmpty(oResult.Data.OrderId) ) return null;
            return oResult.Data.OrderId;    
        }
    }
}

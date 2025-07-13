using BingX.Net.Enums;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx
{
    internal class BingxTrading : IFuturesTrading
    {
        private BingxFutures m_oExchange;

        public BingxTrading(BingxFutures oExchange)
        {
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }

        /// <summary>
        /// Close a position on the exchange.
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null)
        {
            OrderSide eSide = (oPosition.IsLong ? OrderSide.Sell : OrderSide.Buy);
            FuturesOrderType eType = (nPrice != null ? FuturesOrderType.Limit : FuturesOrderType.Market);
            PositionSide ePositionSide = PositionSide.Both; // ( bLong ? PositionSide.Long : PositionSide.Short );
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.Trading.PlaceOrderAsync(
                    oPosition.Symbol.Symbol, // string symbol, 
                    eSide, // OrderSide side, 
                    eType, // FuturesOrderType type, 
                    ePositionSide, // PositionSide positionSide, 
                    oPosition.Quantity, // decimal ? quantity = null, 
                    nPrice, // decimal ? price = null, 
                    closePosition: true // bool ? closePosition = null,
                /*
                bool ? reduceOnly = null, 
                decimal ? stopPrice = null, 
                decimal ? priceRate = null, 
                TakeProfitStopLossMode ? stopLossType = null, 
                decimal ? stopLossStopPrice = null, 
                decimal ? stopLossPrice = null, 
                TriggerType ? stopLossTriggerType = null, 
                bool ? stopLossStopGuaranteed = null, 
                TakeProfitStopLossMode ? takeProfitType = null, 
                decimal ? takeProfitStopPrice = null, 
                decimal ? takeProfitPrice = null, 
                TriggerType ? takeProfitTriggerType = null, 
                bool ? takeProfitStopGuaranteed = null, 
                TimeInForce ? timeInForce = null, 
                bool ? closePosition = null, 
                decimal ? triggerPrice = null, 
                bool ? stopGuaranteed = null, 
                string ? clientOrderId = null, 
                TriggerType ? workingType = null, 
                CancellationToken ct = default(CancellationToken)
                */
                );
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if (oResult.Data.OrderId <= 0)
            {
                return null; // No order ID means failure
            }

            return oResult.Data.OrderId.ToString(); // Order created successfully
        }

        public async Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null)
        {
            OrderSide eSide = bLong ? OrderSide.Buy : OrderSide.Sell;
            FuturesOrderType eType = ( nPrice != null ? FuturesOrderType.Limit : FuturesOrderType.Market);
            PositionSide ePositionSide = PositionSide.Both; // ( bLong ? PositionSide.Long : PositionSide.Short );
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol, // string symbol, 
                    eSide, // OrderSide side, 
                    eType, // FuturesOrderType type, 
                    ePositionSide, // PositionSide positionSide, 
                    nQuantity, // decimal ? quantity = null, 
                    nPrice // decimal ? price = null, 
                    /*
                    bool ? reduceOnly = null, 
                    decimal ? stopPrice = null, 
                    decimal ? priceRate = null, 
                    TakeProfitStopLossMode ? stopLossType = null, 
                    decimal ? stopLossStopPrice = null, 
                    decimal ? stopLossPrice = null, 
                    TriggerType ? stopLossTriggerType = null, 
                    bool ? stopLossStopGuaranteed = null, 
                    TakeProfitStopLossMode ? takeProfitType = null, 
                    decimal ? takeProfitStopPrice = null, 
                    decimal ? takeProfitPrice = null, 
                    TriggerType ? takeProfitTriggerType = null, 
                    bool ? takeProfitStopGuaranteed = null, 
                    TimeInForce ? timeInForce = null, 
                    bool ? closePosition = null, 
                    decimal ? triggerPrice = null, 
                    bool ? stopGuaranteed = null, 
                    string ? clientOrderId = null, 
                    TriggerType ? workingType = null, 
                    CancellationToken ct = default(CancellationToken)
                    */
                );
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null;
            if( oResult.Data.OrderId <= 0)
            {
                return null; // No order ID means failure
            }

            return oResult.Data.OrderId.ToString(); // Order created successfully
        }

        /// <summary>
        /// Close orders
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<bool> CloseOrders(IFuturesSymbol oSymbol)
        {
            var oResult = await m_oExchange.RestClient.PerpetualFuturesApi.Trading.GetOpenOrdersAsync(oSymbol.Symbol);
            if (oResult == null || !oResult.Success) return false;
            if (oResult.Data == null || oResult.Data.Count() <= 0) return true; // No open orders to close
            foreach (var oOrder in oResult.Data)
            {
                var oCloseResult = await m_oExchange.RestClient.PerpetualFuturesApi.Trading.CancelOrderAsync(oSymbol.Symbol, oOrder.OrderId);
                if (oCloseResult == null || !oCloseResult.Success) return false; // Failed to close an order
            }
            return true; // All orders closed successfully
        }
    }
}

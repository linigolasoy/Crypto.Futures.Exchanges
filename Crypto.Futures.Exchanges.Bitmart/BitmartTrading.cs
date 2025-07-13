using BitMart.Net.Enums;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart
{
    internal class BitmartTrading : IFuturesTrading
    {
        private BitmartFutures m_oExchange;
        public BitmartTrading(BitmartFutures oExchange)
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
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Trading.GetOpenOrdersAsync(
                oSymbol.Symbol // string symbol, 
                               // CancellationToken ct = default(CancellationToken)
            );
            if (oResult == null || !oResult.Success) return false;
            if (oResult.Data == null || oResult.Data.Length <= 0) return true; // No open orders to close
            foreach (var oOrder in oResult.Data)
            {
                var oCancelResult = await m_oExchange.RestClient.UsdFuturesApi.Trading.CancelOrderAsync(
                    oSymbol.Symbol, // string symbol, 
                    oOrder.OrderId // long orderId, 
                                   // CancellationToken ct = default(CancellationToken)
                );
                if (oCancelResult == null || !oCancelResult.Success) return false;
            }
            return true; // All orders closed successfully
        }

        public async Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null)
        {
            FuturesSide eSide = oPosition.IsLong ? FuturesSide.SellCloseLong : FuturesSide.BuyCloseShort;
            FuturesOrderType eType = (nPrice == null ? FuturesOrderType.Market : FuturesOrderType.Limit);
            int nQuantityContract = (int)(oPosition.Quantity / oPosition.Symbol.ContractSize); // Convert to contract size, if needed   
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Trading.PlaceOrderAsync(
                    oPosition.Symbol.Symbol, // string symbol, 
                    eSide, // FuturesSide side, 
                    eType, // FuturesOrderType type, 
                    nQuantityContract, // int quantity, 
                    nPrice // decimal ? price = null, 
                           // string ? clientOrderId = null, 
                           // decimal ? leverage = null, 
                           // MarginType ? marginType = null, 
                           // OrderMode ? orderMode = null, 
                           // TriggerPriceType ? presetTakeProfitPriceType = null, 
                           // TriggerPriceType ? presetStopLossPriceType = null, decimal ? presetTakeProfitPrice = null, decimal ? presetStopLossPrice = null, StpMode ? stpMode = null, CancellationToken ct = default(CancellationToken)
                );
            if (oResult == null || !oResult.Success) return null;
            if (oResult.Data == null) return null; // No order created
            if (oResult.Data.OrderId <= 0) return null; // No order id, no order created  
            return oResult.Data.OrderId.ToString(); // Order created successfully
        }

        public async Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null)
        {
            FuturesSide eSide = bLong ? FuturesSide.BuyOpenLong : FuturesSide.SellOpenShort;
            FuturesOrderType eType = (nPrice == null ? FuturesOrderType.Market : FuturesOrderType.Limit);
            int nQuantityContract = (int)(nQuantity / oSymbol.ContractSize); // Convert to contract size, if needed   
            var oResult = await m_oExchange.RestClient.UsdFuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol, // string symbol, 
                    eSide, // FuturesSide side, 
                    eType, // FuturesOrderType type, 
                    nQuantityContract, // int quantity, 
                    nPrice // decimal ? price = null, 
                           // string ? clientOrderId = null, 
                           // decimal ? leverage = null, 
                           // MarginType ? marginType = null, 
                           // OrderMode ? orderMode = null, 
                           // TriggerPriceType ? presetTakeProfitPriceType = null, 
                           // TriggerPriceType ? presetStopLossPriceType = null, decimal ? presetTakeProfitPrice = null, decimal ? presetStopLossPrice = null, StpMode ? stpMode = null, CancellationToken ct = default(CancellationToken)
                );
            if (oResult == null || !oResult.Success) return null;
            if( oResult.Data == null ) return null; // No order created
            if(oResult.Data.OrderId <= 0 ) return null; // No order id, no order created  
            return oResult.Data.OrderId.ToString(); // Order created successfully
        }
    }
}

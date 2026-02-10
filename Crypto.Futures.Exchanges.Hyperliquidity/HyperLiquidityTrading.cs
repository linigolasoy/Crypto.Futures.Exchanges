using Crypto.Futures.Exchanges.Hyperliquidity.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity
{
    internal class HyperLiquidityTrading : IFuturesTrading
    {
        private HyperliquidityExchanges m_oExchange;
        private ConcurrentDictionary<string, decimal> m_aLeverages = new ConcurrentDictionary<string, decimal>();
        public HyperLiquidityTrading(HyperliquidityExchanges oExchange)
        {
            m_oExchange = oExchange;
        }
        public IFuturesExchange Exchange { get => m_oExchange; }


        /// <summary>
        /// Close all open orders for a symbol. This will not close positions, only cancel open orders. To close positions, use ClosePosition method.
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CloseOrders(IFuturesSymbol oSymbol)
        {
            try
            {
                IOrder[]? aOrders = await m_oExchange.Account.GetOrders();
                if (aOrders == null) return true;
                var aToCancel = aOrders.Where(p => p.Symbol.Symbol == oSymbol.Symbol && (p.Status == ModelOrderStatus.New || p.Status == ModelOrderStatus.Placed)).ToArray();
                if( aToCancel.Length <= 0) return true;
                foreach (var oOrder in aToCancel)
                {
                    var oCancel = await m_oExchange.RestClient.FuturesApi.Trading.CancelOrderAsync(oOrder.Symbol.Symbol, long.Parse(oOrder.OrderId));
                    if (oCancel == null || !oCancel.Success) return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger?.Error($"HyperLiquidityTrading.CloseOrders: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Close a position by placing an opposite order. If nPrice is null, it will be a market order, otherwise a limit order. If bFillOrKill is true, the order will be cancelled if it cannot be filled immediately.
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="nPrice"></param>
        /// <param name="bFillOrKill"></param>
        /// <returns></returns>
        public async Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null, bool bFillOrKill = false)
        {
            try
            {

                OrderType eType = (nPrice.HasValue ? HyperLiquid.Net.Enums.OrderType.Limit : HyperLiquid.Net.Enums.OrderType.Market);
                if (nPrice == null)
                {
                    nPrice = await GetPrice(oPosition.Symbol, !oPosition.IsLong);
                }
                if (nPrice == null) return null; // Can't get price, fail the order

                OrderSide eSide = (oPosition.IsLong ? OrderSide.Sell : OrderSide.Sell);

                var oOrder = await m_oExchange.RestClient.FuturesApi.Trading.PlaceOrderAsync(
                    symbol: oPosition.Symbol.Symbol,
                    side: eSide,
                    orderType: eType,
                    quantity: oPosition.Quantity,
                    price: (nPrice == null ? 0 : nPrice.Value),
                    timeInForce: (bFillOrKill ? HyperLiquid.Net.Enums.TimeInForce.ImmediateOrCancel : HyperLiquid.Net.Enums.TimeInForce.GoodTillCanceled)
                    );
                if (oOrder == null || !oOrder.Success || oOrder.Data == null) return null;
                return oOrder.Data.OrderId.ToString();
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger?.Error($"HyperLiquidityTrading.CreateOrder: {ex.Message}");
                return null;
            }
        }


        private async Task<decimal?> GetPrice(IFuturesSymbol oSymbol, bool bLong)
        {
            var oData = m_oExchange.Market.Websocket.DataManager.GetData(oSymbol);
            if (oData != null && oData.LastPrice != null)
            {
                return oData.LastPrice.Price;
            }
            var oSub = await m_oExchange.Market.Websocket.Subscribe(oSymbol, WsMessageType.LastPrice);
            if (oSub == null) return null;
            IWebsocketSymbolData? oDataFound = null;
            int nRetries = 0;
            while (oDataFound == null || oDataFound.LastPrice == null && nRetries < 200)
            {
                await Task.Delay(100);
                oDataFound = m_oExchange.Market.Websocket.DataManager.GetData(oSymbol);
            }

            if (oDataFound == null || oDataFound.LastPrice == null) return null;
            return oDataFound.LastPrice.Price;
        }

        public async Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null, bool bFillOrKill = false)
        {
            try
            {

                OrderType eType = (nPrice.HasValue ? HyperLiquid.Net.Enums.OrderType.Limit : HyperLiquid.Net.Enums.OrderType.Market);
                if (nPrice == null)
                {
                    nPrice = await GetPrice(oSymbol, bLong);
                }
                if (nPrice == null) return null; // Can't get price, fail the order

                OrderSide eSide = (bLong ? OrderSide.Buy : OrderSide.Sell);

                var oOrder = await m_oExchange.RestClient.FuturesApi.Trading.PlaceOrderAsync(
                    symbol: oSymbol.Symbol,
                    side: eSide,
                    orderType: eType,
                    quantity: nQuantity,
                    price: (nPrice == null ? 0 : nPrice.Value),
                    timeInForce: (bFillOrKill ? HyperLiquid.Net.Enums.TimeInForce.ImmediateOrCancel : HyperLiquid.Net.Enums.TimeInForce.GoodTillCanceled)
                    );
                if (oOrder == null || !oOrder.Success || oOrder.Data == null) return null;
                return oOrder.Data.OrderId.ToString();
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger?.Error($"HyperLiquidityTrading.CreateOrder: {ex.Message}");
                return null;
            }
        }
    }
}
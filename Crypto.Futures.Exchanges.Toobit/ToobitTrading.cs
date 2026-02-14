using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toobit.Net.Enums;

namespace Crypto.Futures.Exchanges.Toobit
{
    internal class ToobitTrading : IFuturesTrading
    {

        private ToobitFutures m_oExchange;

        public ToobitTrading(ToobitFutures oExchange)
        {
            m_oExchange = oExchange;
        }

        public IFuturesExchange Exchange { get => m_oExchange; }

        public async Task<bool> CloseOrders(IFuturesSymbol oSymbol)
        {
            try
            {
                var oTaskBuy = m_oExchange.RestClient.UsdtFuturesApi.Trading.CancelAllOrdersAsync(oSymbol.Symbol, OrderSide.Buy);
                var oTaskSell = m_oExchange.RestClient.UsdtFuturesApi.Trading.CancelAllOrdersAsync(oSymbol.Symbol, OrderSide.Sell);

                await Task.WhenAll(oTaskBuy, oTaskSell);
                if (oTaskBuy == null || oTaskSell == null)
                {
                    if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Cancel orders on {oSymbol.ToString()} returned null");
                    return false;
                }
                if (!oTaskBuy.Result.Success || !oTaskSell.Result.Success)
                {
                    if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Cancel orders on {oSymbol.ToString()} returned non success");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Error deleting orders on {oSymbol.ToString()}", ex);
                return false;
            }
        }

        public async Task<string?> ClosePosition(IPosition oPosition, decimal? nPrice = null, bool bFillOrKill = false)
        {
            try
            {
                FuturesOrderSide eSide = (oPosition.IsLong ? FuturesOrderSide.SellClose : FuturesOrderSide.BuyClose);
                FuturesNewOrderType eType = FuturesNewOrderType.Limit;
                PriceType ePriceType = (nPrice == null ? PriceType.Market : PriceType.Input);

                long nQuantityReal = (long)(oPosition.Quantity / oPosition.Symbol.ContractSize);

                var oPlaceOrder = await m_oExchange.RestClient.UsdtFuturesApi.Trading.PlaceOrderAsync(
                    oPosition.Symbol.Symbol,
                    eSide,
                    eType,
                    nQuantityReal,
                    (nPrice == null ? 0 : nPrice.Value),
                    ePriceType
                    );
                if (oPlaceOrder == null || !oPlaceOrder.Success || oPlaceOrder.Data == null)
                {
                    if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Could not place order on {oPosition.Symbol.ToString()}");
                    return null;
                }


                return oPlaceOrder.Data.OrderId.ToString();
            }
            catch (Exception ex)
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Error creating order on {oPosition.Symbol.ToString()}", ex);
                return null;
            }
        }

        public async Task<string?> CreateOrder(IFuturesSymbol oSymbol, bool bLong, decimal nQuantity, decimal? nPrice = null, bool bFillOrKill = false)
        {
            try
            {
                FuturesOrderSide eSide = (bLong ? FuturesOrderSide.BuyOpen : FuturesOrderSide.SellOpen);
                FuturesNewOrderType eType = FuturesNewOrderType.Limit;
                PriceType ePriceType = (nPrice == null ? PriceType.Market : PriceType.Input);

                long nQuantityReal = (long)(nQuantity / oSymbol.ContractSize);

                var oPlaceOrder = await m_oExchange.RestClient.UsdtFuturesApi.Trading.PlaceOrderAsync(
                    oSymbol.Symbol,
                    eSide,
                    eType,
                    nQuantityReal,
                    (nPrice == null ? 0: nPrice.Value),
                    ePriceType
                    );
                if (oPlaceOrder == null || !oPlaceOrder.Success || oPlaceOrder.Data== null)
                {
                    if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Could not place order on {oSymbol.ToString()}");
                    return null;
                }
                return oPlaceOrder.Data.OrderId.ToString();
            }
            catch ( Exception ex) 
            {
                if (m_oExchange.Logger != null) m_oExchange.Logger.Error($"Error creating order on {oSymbol.ToString()}", ex);
                return null;
            }
        }
    }
}

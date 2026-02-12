using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Crypto.Futures.Bot.Interface.ICryptoTrader;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{

    /// <summary>
    /// Trading class   
    /// </summary>
    public class CryptoTrader : ICryptoTrader
    {

        private const int RETRIES = 50;
        public CryptoTrader(
            IExchangeSetup oSetup,
            ICommonLogger oLogger,
            IAccountWatcher oWatcher,
            IQuoter oQuoter) 
        { 
            Money = oSetup.MoneyDefinition.Money;
            Leverage = oSetup.MoneyDefinition.Leverage;
            Logger = oLogger;
            AccountWatcher = oWatcher;
            Quoter = oQuoter;
        }
        public decimal Money { get; }

        public decimal Leverage { get; }

        public int OrderTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICommonLogger Logger { get; }

        public IAccountWatcher AccountWatcher { get; }

        public IQuoter Quoter { get; }

        public async Task<bool> Close(IPosition oPosition, decimal? nPrice = null)
        {
            try
            {
                string? strOrderId = await oPosition.Symbol.Exchange.Trading.ClosePosition(oPosition, nPrice);
                if (strOrderId == null)
                {
                    Logger.Error($"Failed to create limit order on {oPosition.Symbol.ToString()}: Order ID is null");
                    return false;
                }
                // Wait for account watcher
                int nLoops = 0;
                while (nLoops < RETRIES)
                {
                    if (!oPosition.IsOpen) break;

                    await Task.Delay(200);
                    nLoops++;
                }
                if( oPosition.IsOpen )
                {
                    Logger.Error($"Failed to close position on {oPosition.Symbol.ToString()}: Order ID {strOrderId} Socket not found position closed");
                    return false;
                }

                return !oPosition.IsOpen;

            }
            catch (Exception ex)
            {
                Logger.Error($"Error closing position on {oPosition.Symbol.ToString()}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ClosePendingOrders(IFuturesSymbol oSymbol)
        {
            try
            {
                bool bClosed = await oSymbol.Exchange.Trading.CloseOrders(oSymbol);
                if (!bClosed)
                {
                    Logger.Error($"Failed to close pending orders on {oSymbol.ToString()}: CloseOrders returned false");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error closing pending orders on {oSymbol.ToString()}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a limit order and waits for it to be registered in the account watcher. Returns the created order or null if failed.
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nVolume"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IOrder?> CreateLimitOrder(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal nPrice)
        {
            try
            {
                string? strOrderId = await oSymbol.Exchange.Trading.CreateOrder(
                    oSymbol,
                    bLong,
                    nVolume,
                    nPrice);
                if( strOrderId == null )
                {
                    Logger.Error($"Failed to create limit order on {oSymbol.ToString()}: Order ID is null");
                    return null;
                }
                // Wait for account watcher
                int nLoops = 0;
                IOrder? oResult = null;
                while ( nLoops < RETRIES )
                {
                    IOrder[] aOrders = AccountWatcher.GetOrders();
                    oResult = aOrders.FirstOrDefault(o => 
                        o.OrderId == strOrderId && 
                        o.Symbol.Symbol == oSymbol.Symbol && 
                        o.Symbol.Exchange.ExchangeType == o.Symbol.Exchange.ExchangeType );
                    if (oResult != null) break;
                    await Task.Delay(200);
                    nLoops++;
                }

                if (oResult == null )
                {
                    Logger.Error($"Failed to create limit order on {oSymbol.ToString()}: Order ID {strOrderId} Socket not found order");
                    return null;
                }
                return oResult;

            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating limit order on {oSymbol.ToString()}: {ex.Message}");
                return null;
            }
        }

        public async Task<IPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null)
        {
            try
            {
                string? strOrderId = await oSymbol.Exchange.Trading.CreateOrder(
                    oSymbol,
                    bLong,
                    nVolume,
                    nPrice);
                if (strOrderId == null)
                {
                    Logger.Error($"Failed to create limit order on {oSymbol.ToString()}: Order ID is null");
                    return null;
                }
                // Wait for account watcher
                int nLoops = 0;
                IOrder? oOrderResult = null;
                IPosition? oPositionResult = null;
                while (nLoops < RETRIES)
                {
                    if( oOrderResult == null )
                    {
                        IOrder[] aOrders = AccountWatcher.GetOrders();
                        oOrderResult = aOrders.FirstOrDefault(o =>
                            o.OrderId == strOrderId &&
                            o.Symbol.Symbol == oSymbol.Symbol &&
                            o.Symbol.Exchange.ExchangeType == o.Symbol.Exchange.ExchangeType);
                    }
                    if (oOrderResult != null && oOrderResult.Status == ModelOrderStatus.Filled )
                    {
                        IPosition[] aPositions = AccountWatcher.GetPositions();
                        oPositionResult = aPositions.FirstOrDefault(p =>
                            p.Symbol.Symbol == oSymbol.Symbol &&
                            p.Symbol.Exchange.ExchangeType == oSymbol.Exchange.ExchangeType && 
                            p.IsOpen && 
                            p.Quantity == nVolume);
                        if (oPositionResult != null) break;
                    }
                    // Found order filled, 

                    await Task.Delay(200);
                    nLoops++;
                }

                if (oOrderResult == null)
                {
                    Logger.Error($"Failed to open position on {oSymbol.ToString()}: Order ID {strOrderId} Socket not found order filled");
                    return null;
                }

                if( oPositionResult == null )
                {
                    Logger.Error($"Failed to open position on {oSymbol.ToString()}: Order ID {strOrderId} Socket not found position");
                    return null;
                }
                return oPositionResult;

            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating limit order on {oSymbol.ToString()}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> PutLeverage(IFuturesSymbol oSymbol)
        {
            try
            {
                decimal? nLeverage = await oSymbol.Exchange.Account.GetLeverage(oSymbol);
                if (nLeverage != null && nLeverage.Value == Leverage )
                {
                    return true;
                }
                bool bResult = await oSymbol.Exchange.Account.SetLeverage(oSymbol, Leverage);
                if (!bResult)
                {
                    Logger.Error($"Failed to set leverage on {oSymbol.ToString()}: SetLeverage returned false");
                    return false;
                }
                return true;

            }
            catch (Exception ex)
            {
                Logger.Error($"Error setting leverage on {oSymbol.ToString()}: {ex.Message}");
                return false;
            }
        }



        /// <summary>
        /// Update positions from exchange. This is needed to be called after websocket disconnects or if account watcher is not used. If account watcher is used, positions should be updated automatically and this method does not need to be called.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdatePositions()
        {
            try
            {
                IPosition[] aPositions= AccountWatcher.GetPositions();
                IPosition[] aOpenned = aPositions.Where(p => p.IsOpen).ToArray();
                foreach (var oPosition in aOpenned)
                {
                    decimal? nProfit = await Quoter.GetProfit(oPosition);
                    if (nProfit == null) return false;
                    oPosition.Profit = nProfit.Value;
                }
                return true;

            }
            catch (Exception ex)
            {
                Logger.Error($"Error updating profit on trader positions : {ex.Message}");
                return false;
            }
        }
    }
}

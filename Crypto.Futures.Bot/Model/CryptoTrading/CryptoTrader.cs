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

namespace Crypto.Futures.Bot.Model.CryptoTrading
{


    /// <summary>
    /// Crypto trader
    /// </summary>
    internal class CryptoTrader : ICryptoTrader
    {
        private class FakePosition : IPosition
        {
            public FakePosition( ICryptoPosition oPosition )
            {
                Id = oPosition.OrderOpen.OrderId;
                Symbol = oPosition.Symbol;
                CreatedAt = oPosition.OrderOpen.CreatedAt;
                IsLong = oPosition.IsLong;
                Quantity = oPosition.OrderOpen.Quantity;
            }
            public string Id { get; }

            public IFuturesSymbol Symbol { get; }

            public DateTime CreatedAt { get; }

            public DateTime UpdatedAt => throw new NotImplementedException();

            public bool IsLong { get; }

            public bool IsOpen { get => true; }

            public decimal AveragePriceOpen { get; }

            public decimal? PriceClose { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public decimal Quantity { get; }

            public decimal Profit => throw new NotImplementedException();

            public WsMessageType MessageType => throw new NotImplementedException();

            public void Update(IWebsocketMessageBase oMessage)
            {
                throw new NotImplementedException();
            }
        }


        private ConcurrentDictionary<ExchangeType, IBalance> m_aBalances = new ConcurrentDictionary<ExchangeType, IBalance>();  
        private ConcurrentDictionary<int, ICryptoPosition> m_aPositionActive = new ConcurrentDictionary<int, ICryptoPosition>();
        private ConcurrentDictionary<int, ICryptoPosition> m_aPositionClosed = new ConcurrentDictionary<int, ICryptoPosition>();
        public CryptoTrader( ICryptoBot oBot) 
        { 
            Bot = oBot;
            Money = oBot.Setup.MoneyDefinition.Money;
            Leverage = oBot.Setup.MoneyDefinition.Leverage;
        }
        public decimal Money { get; }

        public decimal Leverage { get; }

        public int OrderTimeout { get; set; } = 60;

        public ICryptoBot Bot { get; }

        public IBalance[] Balances { get => m_aBalances.Values.ToArray(); }

        public ICryptoPosition[] PositionsActive { get => m_aPositionActive.Values.ToArray(); }

        public ICryptoPosition[] PositionsClosed { get => m_aPositionClosed.Values.ToArray(); }

        /// <summary>
        /// Close
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Close(ICryptoPosition oPosition, decimal? nPrice = null)
        {
            try
            {
                // TODO: UpdateBalances();
                IPosition oFakePosition = new FakePosition(oPosition);
                string? strOrder = await oPosition.Symbol.Exchange.Trading.ClosePosition( oFakePosition, nPrice);
                if (strOrder == null)
                {
                    Bot.Logger.Error($"Failed to close position for symbol {oPosition.Symbol.ToString()} Long:{oPosition.IsLong}");
                    return false;
                }
                int nDelay = 100;
                int nRetries = OrderTimeout * 1000 / nDelay;

                IOrder? oOrder = null;
                while (nRetries >= 0)
                {
                    await Task.Delay(nDelay);
                    if (oOrder == null)
                    {
                        oOrder = oPosition.Symbol.Exchange.Account.WebsocketPrivate.GetOrder(strOrder);
                    }
                    if (oOrder == null) continue;
                    if (oOrder.Status == ModelOrderStatus.Filled)
                    {
                        ((CryptoPosition)oPosition).OrderClose = oOrder;
                        oPosition.Update();
                        m_aPositionActive.TryRemove(oPosition.Id, out ICryptoPosition? oValue);
                        m_aPositionClosed.TryAdd(oPosition.Id, oPosition);

                        //ICryptoPosition oPosition = new CryptoPosition(this, oData.LastOrderbookPrice, oOrder);
                        Bot.Logger.Info($"{oPosition.ToString()}");
                        return true;
                    }
                    nRetries--;
                }
                // Close all orders
                await oPosition.Symbol.Exchange.Trading.CloseOrders(oPosition.Symbol); // Cancel order if not found
                return false;
            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Error closing position for symbol {oPosition.Symbol.ToString()}: {ex.Message}", ex);
                return false;

            }
        }



        /// <summary>
        /// Update balances
        /// </summary>
        private void UpdateBalances()
        {
            if (m_aBalances.Count == Bot.Exchanges.Length) return;
            try
            {
                foreach( var oExchange in Bot.Exchanges )
                {
                    IBalance? oBalance = null;  
                    if( !m_aBalances.TryGetValue(oExchange.ExchangeType, out oBalance) )
                    {
                        IBalance[] aFound = oExchange.Account.WebsocketPrivate.Balances;
                        if (aFound == null || aFound.Length <= 0) continue;
                        IBalance? oFound = aFound.FirstOrDefault(p => p.Currency == "USDT");
                        if (oFound == null) continue;
                        m_aBalances.TryAdd(oExchange.ExchangeType, oFound);
                    }
                }
            }
            catch( Exception ex )
            {
                Bot.Logger.Error("Error updating balances", ex);
            }
        }


        /// <summary>
        /// Open position
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nVolume"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<ICryptoPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null)
        {
            try
            {
                UpdateBalances();
                string? strOrder = await oSymbol.Exchange.Trading.CreateOrder(oSymbol, bLong, nVolume, nPrice);
                if (strOrder == null )
                {
                    Bot.Logger.Error($"Failed to open position for symbol {oSymbol.ToString()} Long:{bLong}");
                    return null;
                }
                int nDelay = 100;
                int nRetries = OrderTimeout * 1000 / nDelay;
                var oData = oSymbol.Exchange.Market.Websocket.DataManager.GetData(oSymbol);
                if (oData == null || oData.LastOrderbookPrice == null )
                {
                    Bot.Logger.Error($"Failed to get market socket data for {oSymbol.ToString()} Long:{bLong}");
                    return null;
                }

                IOrder? oOrder = null;  
                while (nRetries >= 0)
                {
                    await Task.Delay(nDelay);
                    if( oOrder == null)
                    {
                        oOrder = oSymbol.Exchange.Account.WebsocketPrivate.GetOrder(strOrder);
                    }
                    if (oOrder == null) continue;
                    if( oOrder.Status == ModelOrderStatus.Filled )
                    {
                        ICryptoPosition oPosition = new CryptoPosition(this, oData.LastOrderbookPrice, oOrder);
                        Bot.Logger.Info($"{oPosition.ToString()}");
                        m_aPositionActive.TryAdd(oPosition.Id, oPosition);  
                        return oPosition;
                    }
                    nRetries--;
                }
                await oSymbol.Exchange.Trading.CloseOrders(oSymbol); // Cancel order if not found
                return null;
            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Error opening position for symbol {oSymbol.ToString()}: {ex.Message}", ex);
                return null;

            }
        }

        /// <summary>
        /// Put leverate
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<bool> PutLeverage(IFuturesSymbol oSymbol)
        {
            try
            {
                bool bResult = await oSymbol.Exchange.Account.SetLeverage(oSymbol, Leverage);
                if( !bResult )
                {
                    Bot.Logger.Error($"Could not set leverage on {oSymbol.ToString()} Unknown error");
                }
                return bResult;
            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Could not set leverage on {oSymbol.ToString()}", ex);
            }
            return false;
        }
    }
}

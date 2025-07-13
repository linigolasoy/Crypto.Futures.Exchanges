using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Trading
{
    /*
    /// <summary>
    /// Trader with sockets
    /// </summary>
    internal class TraderSocket : ITrader
    {

        private ConcurrentDictionary<ExchangeType, IBalance> m_aBalances = new ConcurrentDictionary<ExchangeType, IBalance>();


        private const int WAIT_DELAY = 1000;
        public TraderSocket( ITradingBot oBot ) 
        { 
            Bot = oBot;
            Money = oBot.Setup.MoneyDefinition.Money;
            Leverage = oBot.Setup.MoneyDefinition.Leverage;
        }
        public decimal Money { get; }

        public decimal Leverage { get; }

        public ITradingBot Bot { get; }

        public int OrderTimeout { get; set; } = 180;

        public IBalance[] Balances { get => m_aBalances.Values.ToArray(); }

        public ITraderPosition[] ActivePositions => throw new NotImplementedException();

        public ITraderPosition[] ClosedPositions => throw new NotImplementedException();


        /// <summary>
        /// Close position
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<bool> Close(ITraderPosition oPosition, decimal? nPrice = null)
        {
            try
            {
                if( oPosition.Position == null ) return false;
                bool bClose = await oPosition.Symbol.Exchange.Trading.ClosePosition(oPosition.Position, nPrice);
                if (!bClose)
                {
                    Bot.Logger.Error($"Failed to create close order for symbol {oPosition.Symbol.ToString()} Long:{oPosition.Position.IsLong}");
                    return false;

                }
                int nDelay = 100;
                int nRetries = OrderTimeout * 1000 / nDelay;
                while (nRetries >= 0)
                {
                    await Task.Delay(nDelay);
                    if (!oPosition.Position.IsOpen)
                    {
                        ((TraderPosition)oPosition).Profit = oPosition.Position.Profit;
                        Bot.Logger.Info($"  Closed position {oPosition.Id} for {oPosition.Symbol.ToString()} with  profit {oPosition.Profit} with volume {oPosition.Volume} (long: {oPosition.IsLong})");
                        return true;
                    }
                    nRetries--;
                }
                await oPosition.Symbol.Exchange.Trading.CloseOrders(oPosition.Symbol); // Cancel order if not found
                return false;


            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Error closing position {oPosition.Id} for symbol {oPosition.Symbol.ToString()}: {ex.Message}", ex);
                return false;
            }
        }

        private void UpdateBalances()
        {
            if (m_aBalances.Count > 0) return;
            foreach( var oExchange in Bot.Exchanges )
            {
                IBalance[] aBalances = oExchange.Account.WebsocketPrivate.Balances;
                IBalance? oFound = aBalances.FirstOrDefault(p => p.Currency == "USDT");
                if (oFound == null) continue;
                m_aBalances.TryAdd(oExchange.ExchangeType, oFound); 
            }
        }

        /// <summary>
        /// Try to open new position
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <param name="bLong"></param>
        /// <param name="nVolume"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<ITraderPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null)
        {
            try
            {
                UpdateBalances();
                bool bOrder = await oSymbol.Exchange.Trading.CreateOrder(oSymbol, bLong, nVolume, nPrice);
                if (!bOrder)
                {
                    Bot.Logger.Error($"Failed to open position for symbol {oSymbol.ToString()} Long:{bLong}");
                    return null;
                }
                int nDelay = 100;
                int nRetries = OrderTimeout * 1000 / nDelay;
                while (nRetries >= 0)
                {
                    await Task.Delay(nDelay);
                    IPosition[] aPositions = oSymbol.Exchange.Account.WebsocketPrivate.Positions;
                    if (aPositions != null && aPositions.Length > 0)
                    {
                        IPosition? oFound = aPositions.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol && p.IsLong == bLong && p.Quantity == nVolume);
                        if (oFound != null)
                        {
                            TraderPosition oPosition = new TraderPosition(oSymbol, bLong, nVolume, oFound.AveragePriceOpen);
                            oPosition.Position = oFound;
                            
                            Bot.Logger.Info($"  Opened position {oPosition.Id} for {oSymbol.ToString()} at price {oFound.AveragePriceOpen} with volume {nVolume} (long: {bLong})");
                            return oPosition;
                        }
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
        /// Put leverage on symbol
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<bool> PutLeverage(IFuturesSymbol oSymbol)
        {
            try
            {
                decimal? nLeverage = await oSymbol.Exchange.Account.GetLeverage(oSymbol);
                if (nLeverage != null && nLeverage == this.Leverage) return true;
                bool bResult = await oSymbol.Exchange.Account.SetLeverage(oSymbol, this.Leverage);
                if (!bResult)
                {
                    Bot.Logger.Error($"Failed to set leverage for symbol {oSymbol.ToString()} to {this.Leverage}");
                    return false;
                }
                await Task.Delay(WAIT_DELAY); // Wait for the leverage to be set    
                return bResult;
            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Error setting leverage for symbol {oSymbol.ToString()}: {ex.Message}", ex);
                return false;
            }
        }

        public bool Update()
        {
            throw new NotImplementedException();
        }
    }
    */
}

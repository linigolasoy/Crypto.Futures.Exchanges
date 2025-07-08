using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Trading
{

    /// <summary>
    /// Trader position implementation for the IPosition interface.
    /// </summary>
    internal class TraderIPosition : IPosition
    {
        public TraderIPosition(ITraderPosition oPosition)
        {
            Id = oPosition.Id.ToString();
            Symbol = oPosition.Symbol;
            CreatedAt = oPosition.DateOpen;
            UpdatedAt = DateTime.Now;
            IsLong = oPosition.IsLong;
            IsOpen = (oPosition.DateClose == null);
            AveragePriceOpen = oPosition.PriceOpen;
            Quantity = oPosition.Volume;
        }
        public string Id { get; }
        public IFuturesSymbol Symbol { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }
        public bool IsLong { get; }
        public bool IsOpen { get; }
        public decimal AveragePriceOpen { get; }
        public decimal? PriceClose { get; set; } = null;
        public decimal Quantity { get; }
    }

    /// <summary>
    /// Trader implementation without a socket connection.  
    /// </summary>
    public class TraderNoSocket : ITrader
    {
        private const int WAIT_DELAY = 1000; 

        private ConcurrentDictionary<ExchangeType, IBalance> m_aBalances = new ConcurrentDictionary<ExchangeType, IBalance>();
        public TraderNoSocket(ITradingBot oBot)
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
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Close(ITraderPosition oPosition, decimal? nPrice = null)
        {
            try
            {
                IPosition oIPosition = new TraderIPosition(oPosition);
                bool bClose = await oIPosition.Symbol.Exchange.Trading.ClosePosition(oIPosition, nPrice);
                if (!bClose)
                {
                    Bot.Logger.Error($"Failed to create close order for symbol {oIPosition.Symbol.ToString()} Long:{oIPosition.IsLong}");
                    return false;
                }
                int nRetries = OrderTimeout * 1000 / WAIT_DELAY;
                while (nRetries >= 0)
                {
                    await Task.Delay(WAIT_DELAY);
                    IPosition[]? aPositions = await oIPosition.Symbol.Exchange.Account.GetPositions();
                    if (aPositions != null)
                    {
                        IPosition? oFound = aPositions.FirstOrDefault(p => p.Symbol.Symbol == oIPosition.Symbol.Symbol && p.IsLong == oIPosition.IsLong && p.Quantity == oIPosition.Quantity);
                        if (oFound == null)
                        {
                            IPosition[]? aHistory = await oIPosition.Symbol.Exchange.Account.GetPositionHistory(oIPosition.Symbol);
                            if (aHistory == null) continue;
                            IPosition? oClosed = aHistory.OrderByDescending(p=> p.CreatedAt).FirstOrDefault(p => p.Symbol.Symbol == oIPosition.Symbol.Symbol && p.IsLong == oIPosition.IsLong && p.Quantity == oIPosition.Quantity && p.CreatedAt > oPosition.DateOpen);
                            if (oClosed == null) continue;
                            ((TraderPosition)oPosition).PriceClose = oClosed.PriceClose!.Value;
                            Bot.Logger.Info($"  Closed position {oPosition.Id} for {oPosition.Symbol.ToString()} at price {oPosition.PriceClose} with volume {oPosition.Volume} (long: {oPosition.IsLong})");
                            return true;
                        }
                    }
                    nRetries--;
                }
                await oIPosition.Symbol.Exchange.Trading.CloseOrders(oIPosition.Symbol); // Cancel order if not found
                return false;


            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Error closing position {oPosition.Id} for symbol {oPosition.Symbol.ToString()}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Ttrade
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
                bool bOrder = await oSymbol.Exchange.Trading.CreateOrder(oSymbol, bLong, nVolume, nPrice); 
                if(!bOrder)
                {
                    Bot.Logger.Error($"Failed to open position for symbol {oSymbol.ToString()} Long:{bLong}");
                    return null;
                }
                int nRetries = OrderTimeout * 1000 / WAIT_DELAY;
                while (nRetries >= 0)
                {
                    await Task.Delay(WAIT_DELAY);
                    IPosition[]? aPositions = await oSymbol.Exchange.Account.GetPositions();
                    if ( aPositions != null )
                    {
                        IPosition? oFound = aPositions.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol && p.IsLong == bLong && p.Quantity == nVolume);
                        if(oFound != null)
                        {
                            ITraderPosition oPosition = new TraderPosition(oSymbol, bLong, nVolume, oFound.AveragePriceOpen);
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
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> PutLeverage(IFuturesSymbol oSymbol)
        {
            try
            {
                decimal? nLeverage = await oSymbol.Exchange.Account.GetLeverage(oSymbol);
                if (nLeverage != null && nLeverage == this.Leverage) return true;
                bool bResult = await oSymbol.Exchange.Account.SetLeverage(oSymbol, this.Leverage);
                if(!bResult)
                {
                    Bot.Logger.Error($"Failed to set leverage for symbol {oSymbol.ToString()} to {this.Leverage}");
                    return false;
                }
                return bResult; 
            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Error setting leverage for symbol {oSymbol.ToString()}: {ex.Message}", ex);
                return false;
            }
            throw new NotImplementedException();
        }

        public bool Update()
        {
            throw new NotImplementedException();
        }
    }
}

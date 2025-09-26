using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.Arbitrage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{

    /// <summary>
    /// Arbitrage position
    /// </summary>
    internal class ArbitragePosition : IArbitragePosition
    {

        private const int WAIT_DELAY = 100;
        private const int SECONDS_WAIT = 10;
        private const int CHECKS_NEEDED = 5;

        private bool m_bNeedsCancelOpen = false;
        private bool m_bNeedsCancelClose = false;
        public ArbitragePosition( ICryptoBot oBot, IArbitrageChance oChance ) 
        { 
            Bot = oBot;
            Chance = oChance;
            Runner = RunTask();
        }
        private static int m_nLastId = 0;
        public ICryptoBot Bot { get; }

        public int Id { get; } = m_nLastId++;

        public ArbitragePositionStatus Status { get; private set; }

        public IArbitrageChance Chance { get; }

        public decimal Amount { get; private set; } = 0;

        public decimal Percent { get; private set; } = 0;

        public decimal PercentProfit { get; private set; } = 0;

        public decimal Profit { get; private set; } = 0;

        public ICryptoPosition? PositionLong { get; private set; } = null;

        public ICryptoPosition? PositionShort { get; private set; } = null;

        public Task Runner { get; }

        private DateTime m_dLastLog = DateTime.Now;

        private decimal? CalculateQuantity(IArbitrageChance oChance)
        {
            decimal nPrice = Math.Max(oChance.PriceShort, oChance.PriceLong);
            decimal nMoney = Bot.Setup.MoneyDefinition.Money * Bot. Setup.MoneyDefinition.Leverage;
            int nDecimals = (oChance.OrderbookLong.Symbol.QuantityDecimals < oChance.OrderbookShort.Symbol.QuantityDecimals ?
                    oChance.OrderbookLong.Symbol.QuantityDecimals :
                    oChance.OrderbookShort.Symbol.QuantityDecimals);
            if (nPrice <= 0) return null;
            decimal nQuantity = Math.Round(nMoney / nPrice, nDecimals);
            return nQuantity;
        }

        private async Task<bool> Check()
        {
            int nRetries = SECONDS_WAIT * 1000 / WAIT_DELAY;
            int nPassed = 0;
            int nMaxPassed = 0;
            while (nRetries > 0)
            {
                if (Chance.Check())
                {
                    decimal? nQuantity = CalculateQuantity(Chance);
                    if (nQuantity != null && nQuantity.Value > 0)
                    {
                        nPassed++;
                        if( nPassed > nMaxPassed ) nMaxPassed = nPassed;    
                        if (nPassed >= CHECKS_NEEDED)
                        {
                            Amount = nQuantity.Value;
                            return true;
                        }
                    }
                    else nPassed = 0;
                }
                else nPassed = 0;
                await Task.Delay(WAIT_DELAY);
                nRetries--;
            }
            Bot.Logger.Info($" Failed Check on {Chance.ToString()} Max Passed {nMaxPassed}!!!");
            return false;
        }

        /// <summary>
        /// Check if cancel order
        /// </summary>
        /// <returns></returns>
        private bool OnCancelOpen()
        {
            return m_bNeedsCancelOpen;
        }

        /// <summary>
        /// Check if cancel order close
        /// </summary>
        /// <returns></returns>
        private bool OnCancelClose()
        {
            return m_bNeedsCancelClose;
        }

        /// <summary>
        /// Try to open
        /// </summary>
        /// <returns></returns>
        private async Task<bool> TryOpen()
        {
            try
            {
                // List<Task<ICryptoPosition?>> aTasks = new List<Task<ICryptoPosition?>>();
                Bot.Logger.Info($"====> Try Open {Chance.ToString()}");
                Task<ICryptoPosition?> oTaskLong = Bot.Trader.OpenFillOrKill(Chance.OrderbookLong.Symbol, true, Amount, Chance.PriceLong);
                Task<ICryptoPosition?> oTaskShort = Bot.Trader.OpenFillOrKill(Chance.OrderbookShort.Symbol, false, Amount, Chance.PriceShort);
                // Task<ICryptoPosition?> oTaskLong = Bot.Trader.Open(Chance.OrderbookLong.Symbol, true, Amount, OnCancelOpen, Chance.PriceLong);
                // Task<ICryptoPosition?> oTaskShort = Bot.Trader.Open(Chance.OrderbookShort.Symbol, false, Amount, OnCancelOpen, Chance.PriceShort);

                decimal nMoney = Bot.Setup.MoneyDefinition.Money * Bot.Setup.MoneyDefinition.Leverage;
                decimal nMaxProfit = nMoney * Bot.Setup.Arbitrage.ClosePercent / 100.0M;
                decimal nMaxLoss = -nMaxProfit;
                while (true)
                {
                    if (oTaskLong.IsCompleted && oTaskShort.IsCompleted) break;
                    if( oTaskLong.IsCompleted )
                    {
                        PositionLong = oTaskLong.Result;
                        if( PositionLong != null )
                        {
                            PositionLong.Update();
                            // if( PositionLong.Profit < nMaxLoss || PositionLong.Profit > nMaxProfit ) m_bNeedsCancelOpen = true;
                        }
                    }
                    if (oTaskShort.IsCompleted)
                    {
                        PositionShort = oTaskShort.Result;
                        if (PositionShort != null)
                        {
                            PositionShort.Update();
                            // if (PositionShort.Profit < nMaxLoss || PositionShort.Profit > nMaxProfit) m_bNeedsCancelOpen = true;
                        }
                    }
                    await Task.Delay(100);
                }

                PositionLong = oTaskLong.Result;
                PositionShort = oTaskShort.Result;
                // No position on any side, just leave chance

                if(PositionLong == null && PositionShort == null ) return false;   
                bool bCancel = false;   
                // Close market short
                if(PositionLong == null && PositionShort != null)
                {
                    bool bClosed = await Bot.Trader.CloseFillOrKill(PositionShort);
                    if (!bClosed) throw new Exception("Error closing short position on market");
                    bCancel = true; 
                    // return false;   
                }
                // Close market short
                if (PositionLong != null && PositionShort == null)
                {
                    bool bClosed = await Bot.Trader.Close(PositionLong);
                    if (!bClosed) throw new Exception("Error closing long position on market");
                    bCancel = true;
                    // return false;
                }
                decimal nProfit = 0;
                if (PositionLong != null) { PositionLong.Update(); nProfit += PositionLong.Profit; }
                if (PositionShort != null) { PositionShort.Update(); nProfit += PositionShort.Profit; }
                Profit = nProfit;

                if( bCancel )
                {
                    Bot.Logger.Info($"====> Cancelled {Chance.ToString()} with profit {nProfit}");
                    return false;
                }
                Bot.Logger.Info($"====> Openned {Chance.ToString()}");
                return true;
            }
            catch ( Exception ex )
            {
                Bot.Logger.Error($"TryOpen Error on {Chance.Currency}", ex);
            }
            return false;
        }

        private void DoLog()
        {
            if (PositionShort == null || PositionLong == null) return;

            decimal nProfit = PositionShort.Profit + PositionLong.Profit;
            decimal nMoney = Bot.Setup.MoneyDefinition.Money * Bot.Setup.MoneyDefinition.Leverage;
            decimal nProfitPercent = Math.Round(100.0M * nProfit / nMoney,2);

            DateTime dNow = DateTime.Now;
            double nMinutes = (dNow - m_dLastLog).TotalMinutes;
            if( nMinutes >= 1 )
            {
                Bot.Logger.Info($" {Chance.Currency} Profit [{nProfit} ({nProfitPercent}%)]");
                m_dLastLog = dNow;
            }
        }

        /// <summary>
        /// Try to close position
        /// </summary>
        /// <returns></returns>
        private async Task<bool> TryClose()
        {
            if (PositionLong == null || PositionShort == null) return false;
            try
            {
                decimal nMoney = Bot.Setup.MoneyDefinition.Money * Bot.Setup.MoneyDefinition.Leverage;
                decimal nDesiredProfit = nMoney * Bot.Setup.Arbitrage.ClosePercent / 100.0M;
                while (true)
                {
                    await Task.Delay(100);
                    if (!PositionLong.Update() || !PositionShort.Update()) continue;
                    decimal nProfit = PositionShort.Profit + PositionLong.Profit;
                    if (nProfit >= nDesiredProfit)
                    {
                        Bot.Logger.Info($"====> Trying to close {Chance.ToString()}");
                        Task<bool> oTaskLong = Bot.Trader.Close(PositionLong, OnCancelClose, PositionLong.LastPrice);
                        Task<bool> oTaskShort = Bot.Trader.Close(PositionShort, OnCancelClose, PositionShort.LastPrice);
                        int nLoops = 0;
                        int nMaxLoops = 200;
                        while( true )
                        {
                            if (oTaskLong.IsCompleted && oTaskShort.IsCompleted) break;
                            PositionLong.Update(); PositionShort.Update();
                            decimal nNewProfit = PositionLong.Profit + PositionShort.Profit;
                            if ( oTaskLong.IsCompleted && nLoops > nMaxLoops )
                            {
                                if( nNewProfit > nDesiredProfit && nNewProfit < -nDesiredProfit ) m_bNeedsCancelClose = true;
                            }
                            if (oTaskShort.IsCompleted && nLoops > nMaxLoops)
                            {
                                if (nNewProfit > nDesiredProfit && nNewProfit < -nDesiredProfit) m_bNeedsCancelClose = true;
                            }
                            await Task.Delay(100);
                            nLoops++;   
                        }

                        bool bClosedLong = oTaskLong.Result;
                        bool bClosedShort = oTaskShort.Result;

                        if (!bClosedLong)
                        {
                            bClosedLong = await Bot.Trader.Close(PositionLong);
                        }
                        if (!bClosedShort)
                        {
                            bClosedShort = await Bot.Trader.Close(PositionShort);
                        }
                        if (bClosedLong && bClosedShort)
                        {
                            PositionLong.Update();
                            PositionShort.Update();
                            Profit = PositionShort.Profit + PositionLong.Profit;
                            Bot.Logger.Info($"====> CLOSED {Chance.ToString()} WITH PROFIT [{Profit}]");
                        }
                        Status = ArbitragePositionStatus.Closed;
                        return true;
                    }
                    else
                    {
                        DoLog();
                    }
                }
            }
            catch (Exception ex)
            {
                Bot.Logger.Error($"Error trying to close {Chance.ToString()}", ex);
                return false;
            }
        }

        /// <summary>
        /// Position loop task
        /// </summary>
        /// <returns></returns>
        private async Task RunTask()
        {
            try
            {
                Bot.Logger.Info($" Acting on chance {Chance.ToString()}");
                bool bCheck = await Check();    
                if( !bCheck ) {  Status = ArbitragePositionStatus.Canceled; return; }
                Bot.Logger.Info($" Check ok on {Chance.ToString()} !!!");

                bool bOpen = await TryOpen();
                if( !bOpen ) { Status = ArbitragePositionStatus.Canceled; return; }

                bool bClose = await TryClose(); 
                if( bClose )
                {
                    Status = ArbitragePositionStatus.Closed;
                }
                else Status = ArbitragePositionStatus.Canceled;
            }
            catch (Exception ex)
            {
                Bot.Logger.Error($" Error on ArbitragePosition {Chance.ToString()}: {ex.Message}", ex );
                Status = ArbitragePositionStatus.Canceled;  
            }
        }
    }
}

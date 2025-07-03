using Crypto.Futures.Bot.Arbitrage.Model;
using Crypto.Futures.Bot.Trading;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{
    /// <summary>
    /// Arbitrage bot implementation
    /// </summary>
    internal class ArbitrageBot : ITradingBot
    {

        private Task? m_oMainLoop = null;
        private IArbitrageFinder? m_oFinder = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private decimal m_nTotalProfit = 0;
        private int m_nWin = 0;
        private int m_nTotal = 0;
        private int m_nOpen = 0;

        private DateTime m_dLastBalance = DateTime.Now;

        public ArbitrageBot( IExchangeSetup oSetup, ICommonLogger oLogger, bool bPaperTrading ) 
        { 
            Setup = oSetup;
            Logger = oLogger;
            Trader = new PaperTrader(this);
        }
        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public ITrader Trader { get; }

        private ConcurrentDictionary<string, IArbitrageChance> m_aChances = new ConcurrentDictionary<string, IArbitrageChance>();
        // private ConcurrentDictionary<string, int> m_aChancesCount = new ConcurrentDictionary<string, int>();
        private ConcurrentBag<IArbitrageChance> m_aClosed = new ConcurrentBag<IArbitrageChance>();

        /// <summary>
        /// Starts boot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            Logger.Info("ArbitrageBot starting...");
            if (m_oMainLoop != null)
            {
                await Stop();
            }
            m_oFinder = new ArbitrageFinder(this);

            m_oCancelSource = new CancellationTokenSource();
            bool bResult = await m_oFinder.Start();

            /*
            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();

            foreach (var eType in Setup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(Setup, eType, Logger);
                aExchanges.Add(oExchange);
            }
            m_aExchanges = aExchanges.ToArray();
            */
            m_oMainLoop = MainLoop();

            Logger.Info("ArbitrageBot started...");
            await Task.Delay(1000);
            return true;
        }

        /// <summary>
        /// Stop bot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            Logger.Info("ArbitrageBot stopping...");
            if ( m_oFinder != null)
            {
                await m_oFinder.Stop();
                m_oFinder = null;
            }
            if (m_oMainLoop != null)
            {
                m_oCancelSource.Cancel();
                await m_oMainLoop;
                m_oMainLoop = null; 
                await Task.Delay(1000);
            }
            Logger.Info("ArbitrageBot stopped...");
            return true;
        }


        private decimal? CalculateQuantity( IArbitrageChance oChance )
        {
            if(oChance.LongData.WsSymbolData == null ) { Logger.Info("QTY1"); return null; }
            if (oChance.ShortData.WsSymbolData == null) { Logger.Info("QTY2"); return null; }
            if (oChance.LongData.WsSymbolData.LastPrice == null) { Logger.Info("QTY3"); return null; }
            if (oChance.ShortData.WsSymbolData.LastPrice == null) { Logger.Info("QTY4"); return null; }
            decimal nPrice = Math.Max(oChance.LongData.WsSymbolData!.LastPrice!.Price, oChance.ShortData.WsSymbolData!.LastPrice!.Price);
            decimal nMoney = Setup.MoneyDefinition.Money * Setup.MoneyDefinition.Leverage;
            int nDecimals = (oChance.LongData.Symbol.QuantityDecimals < oChance.ShortData.Symbol.QuantityDecimals ?
                    oChance.LongData.Symbol.QuantityDecimals :
                    oChance.ShortData.Symbol.QuantityDecimals);
            if( nPrice <= 0 ) { Logger.Info("QTY5"); return null; }
            decimal nQuantity = Math.Round(nMoney / nPrice, nDecimals); 
            return nQuantity;
        }


        private void CloseChance(IArbitrageChance oChance, ChanceStatus eStatus, string strLog )
        {
            oChance.Status = eStatus;
            IArbitrageChance? oRemove = null;
            m_aChances.TryRemove(oChance.Currency, out oRemove);
            Logger.Info($"   Removed {oChance.ToString()} Status {eStatus.ToString()}, Reason [{strLog}]");
        }

        /// <summary>
        /// Main chance task
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task ChanceTask( IArbitrageChance oChance )
        {
            oChance.Status = ChanceStatus.Handling;
            Logger.Info($"Acting on chance {oChance.ToString()}");
            try
            {
                int nRetries = 100;
                decimal nMaxPercent = -100.0M;
                while( nRetries >= 0 )
                {
                    if( oChance.Update() )
                    {
                        if( oChance.Percentage > nMaxPercent ) nMaxPercent = oChance.Percentage;

                        if (oChance.Percentage >= Setup.Arbitrage.MinimumPercent )
                        {
                            break;
                        }
                    }

                    nRetries--; 
                    await Task.Delay(100); 
                }

                if( nRetries <= 0 && oChance.Percentage < Setup.Arbitrage.MinimumPercent ) 
                { 
                    CloseChance(oChance, ChanceStatus.Canceled, "Retries"); 
                    return; 
                }   
                decimal? nQuantity = CalculateQuantity( oChance );
                if( nQuantity == null ) { CloseChance( oChance, ChanceStatus.Canceled, "Quantity"); return; }
                // Buy
                List<Task<ITraderPosition?>> aTasks = new List<Task<ITraderPosition?>>();
                aTasks.Add(Trader.Open(oChance.LongData.Symbol, true, nQuantity.Value));
                aTasks.Add(Trader.Open(oChance.ShortData.Symbol, false, nQuantity.Value));
                await Task.WhenAll( aTasks );
                oChance.LongData.Position = aTasks[0].Result;
                oChance.ShortData.Position = aTasks[1].Result;
                if( oChance.ShortData.Position == null || oChance.LongData.Position == null )
                {
                    Logger.Warning($"   Could not open position on {oChance.ToString()}");
                    CloseChance( oChance, ChanceStatus.Canceled, "OpenError"); 
                    return;
                }
                Logger.Info($"   Position open on {oChance.ToString()}");
                oChance.Status = ChanceStatus.Open;
                m_nOpen++;
                decimal nMoney = Setup.MoneyDefinition.Money * Setup.MoneyDefinition.Leverage;
                    //
                decimal nDesired = nMoney * Setup.Arbitrage.ClosePercent / 100.0M;
                while (oChance.Status == ChanceStatus.Open) 
                {
                    oChance.LongData.Position.Update();
                    oChance.ShortData.Position.Update();

                    decimal nProfit = oChance.LongData.Position.Profit + oChance.ShortData.Position.Profit;
                    decimal nPercentProfit = Math.Round(100.0M * nProfit / nMoney, 3);

                    DateTime dDateOpen = oChance.DateTime;
                    DateTime dNow = DateTime.Now;
                    bool bClose = false;
                    if ((dNow - dDateOpen).TotalMinutes > 30)
                    {
                        Logger.Warning($"     {oChance.ToString()} [{dDateOpen.ToShortTimeString()}] Time out on position");
                        bClose = true;
                    }
                    // Close
                    if (nProfit >= nDesired /*|| nProfit <= -nDesired*/ || bClose)
                    {
                        // oChance.Profit = (oChance.LongData.Position.Profit - oChance.ShortData.Position.Profit);
                        Logger.Info($"====<  Trying to close {oChance.ToString()} on Profit {Math.Round(nProfit, 2)}");
                        List<Task<bool>> aCloseTasks = new List<Task<bool>>();
                        aCloseTasks.Add(Trader.Close(oChance.LongData.Position));
                        aCloseTasks.Add(Trader.Close(oChance.ShortData.Position));
                        await Task.WhenAll(aCloseTasks);
                        oChance.Profit = (oChance.LongData.Position.Profit + oChance.ShortData.Position.Profit);
                        Logger.Info($"====<  Closed {oChance.ToString()} => Profit {Math.Round(oChance.Profit.Value, 2)}");
                        m_nTotalProfit += oChance.Profit.Value;
                        m_nTotal++;
                        m_nOpen--;
                        if (oChance.Profit > 0) m_nWin++;
                        oChance.Status = ChanceStatus.Closed;
                        Logger.Info($"[{Math.Round(m_nTotalProfit, 2)}] TOTAL PROFIT ({m_nWin}/{m_nTotal}) Still Openned {m_nOpen}");
                    }
                    else
                    {
                        if (nProfit > oChance.MaxProfit)
                        {
                            oChance.MaxProfit = nProfit;
                            Logger.Info($"     {oChance.ToString()} => ({nPercentProfit}% / {oChance.Percentage}%) Profit {nProfit}");
                        }
                        else
                        {
                            if ((dNow - oChance.LastLog).TotalMinutes > 1)
                            {
                                oChance.LastLog = dNow;
                                Logger.Info($"     {oChance.ToString()} => ({nPercentProfit}% / {oChance.Percentage}%) Profit {nProfit}");

                            }
                        }
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"  Error on chance {oChance.ToString()}", e);
            }
            if( oChance.Status != ChanceStatus.Closed ) oChance.Status = ChanceStatus.Canceled;
            m_aClosed.Add( oChance );
            IArbitrageChance? oRemove = null;   
            m_aChances.TryRemove(oChance.Currency, out oRemove);    

        }

        /// <summary>
        /// Update balances for all exchanges   
        /// </summary>
        private void UpdateBalances()
        {
            DateTime dNow = DateTime.Now;
            if ((dNow - m_dLastBalance).TotalMinutes < 5) return; // Update every minute
            m_dLastBalance = dNow;
            Trader.Update();
            IBalance[] aBalances = Trader.Balances;
            decimal nTotal = aBalances.Sum(b => b.Balance); 
            Logger.Info($"Balances TOTAL {nTotal}");
            foreach (IBalance oBalance in aBalances)
            {
                Logger.Info($"  {oBalance.Exchange.ExchangeType} => {Math.Round(oBalance.Balance, 2)} ({Math.Round(oBalance.Locked, 2)} locked)");
            }   

        }

        private bool CanTrade(IArbitrageChance oChance)
        {
            IArbitrageChance? oFound = null;
            if (m_aChances.TryGetValue(oChance.Currency, out oFound)) return false;

            int nChancesLong = m_aChances.Where(p=> p.Value.LongData.Symbol.Exchange.ExchangeType == oChance.LongData.Symbol.Exchange.ExchangeType ||
                                                    p.Value.ShortData.Symbol.Exchange.ExchangeType == oChance.LongData.Symbol.Exchange.ExchangeType ).Count();

            int nChancesShort = m_aChances.Where(p => p.Value.LongData.Symbol.Exchange.ExchangeType == oChance.ShortData.Symbol.Exchange.ExchangeType ||
                                                     p.Value.ShortData.Symbol.Exchange.ExchangeType == oChance.ShortData.Symbol.Exchange.ExchangeType).Count();
            if (nChancesLong >= 2) return false;
            if( nChancesShort >= 2 ) return false;
            return true;    

        }
        /// <summary>
        /// TODO: Main background loop
        /// </summary>
        /// <returns></returns>
        internal async Task MainLoop()
        {
            while( !m_oCancelSource.IsCancellationRequested )
            {
                if( m_oFinder != null )
                {
                    IArbitrageChance[]? aChances = await m_oFinder.FindValidChances();
                    if (aChances != null && aChances.Length > 0)
                    {
                        foreach (var oChance in aChances)
                        {
                            if( !CanTrade(oChance)) continue;   
                            /*
                            {
                                if (oFound.Status != ArbitrageStatus.Canceled && oFound.Status != ArbitrageStatus.Closed) continue;
                                m_aChances[oChance.Currency] = oChance;
                                oChance.TradeTask = ChanceTask(oChance);
                            }
                            */
                            if ( m_aChances.TryAdd(oChance.Currency, oChance) )
                            {
                                oChance.TradeTask = ChanceTask( oChance );
                            }
                        }
                    }
                }
                UpdateBalances();
                await Task.Delay(200); 
            }
        }
    }
}

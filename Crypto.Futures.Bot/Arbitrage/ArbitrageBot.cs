using Crypto.Futures.Bot.Arbitrage.Model;
using Crypto.Futures.Bot.Trading;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Factory;
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
        private ConcurrentDictionary<string, int> m_aChancesCount = new ConcurrentDictionary<string, int>();
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
            decimal nPrice = Math.Max(oChance.LongData.WsSymbolData!.LastPrice!.Price, oChance.ShortData.WsSymbolData!.LastPrice!.Price);
            decimal nMoney = Setup.MoneyDefinition.Money * Setup.MoneyDefinition.Leverage;
            int nDecimals = (oChance.LongData.Symbol.QuantityDecimals < oChance.ShortData.Symbol.QuantityDecimals ?
                    oChance.LongData.Symbol.QuantityDecimals :
                    oChance.ShortData.Symbol.QuantityDecimals);
            if( nPrice <= 0 ) { oChance.Status = ArbitrageStatus.Canceled; return null; }
            decimal nQuantity = Math.Round(nMoney / nPrice, nDecimals); 
            return nQuantity;
        }

        /// <summary>
        /// Main chance task
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task ChanceTask( IArbitrageChance oChance )
        {
            oChance.Status = ArbitrageStatus.Handling;
            Logger.Info($"Acting on chance {oChance.ToString()}");
            try
            {
                int nRetries = 100;
                while( !oChance.IsDataValid && nRetries >= 0) { nRetries--; await Task.Delay(100); }

                if( nRetries <= 0 ) { oChance.Status = ArbitrageStatus.Canceled; return; }   
                decimal? nQuantity = CalculateQuantity( oChance );
                if( nQuantity == null ) { oChance.Status = ArbitrageStatus.Canceled; return; }
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
                    oChance.Status = ArbitrageStatus.Canceled; 
                    return;
                }
                Logger.Info($"   Position open on {oChance.ToString()}");
                oChance.Status = ArbitrageStatus.Open;
                m_nOpen++;
                decimal nMoney = Setup.MoneyDefinition.Money * Setup.MoneyDefinition.Leverage;
                    //
                decimal nDesired = nMoney * 1.0M / 100.0M;
                while (oChance.Status == ArbitrageStatus.Open) 
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
                        List<Task<bool>> aCloseTasks = new List<Task<bool>>();
                        aCloseTasks.Add(Trader.Close(oChance.LongData.Position));
                        aCloseTasks.Add(Trader.Close(oChance.ShortData.Position));
                        await Task.WhenAll(aCloseTasks);
                        oChance.Profit = (oChance.LongData.Position.Profit - oChance.ShortData.Position.Profit);
                        Logger.Info($"====<  Closed {oChance.ToString()} => Profit {Math.Round(oChance.Profit.Value, 2)}");
                        m_nTotalProfit += oChance.Profit.Value;
                        m_nTotal++;
                        m_nOpen--;
                        if (oChance.Profit > 0) m_nWin++;
                        oChance.Status = ArbitrageStatus.Closed;
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
            if( oChance.Status != ArbitrageStatus.Closed ) oChance.Status = ArbitrageStatus.Canceled;
            m_aClosed.Add( oChance );
            IArbitrageChance? oRemove = null;   
            m_aChances.TryRemove(oChance.Currency, out oRemove);    

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
                            IArbitrageChance? oFound = null;
                            if (m_aChances.TryGetValue(oChance.Currency, out oFound)) continue;
                            /*
                            {
                                if (oFound.Status != ArbitrageStatus.Canceled && oFound.Status != ArbitrageStatus.Closed) continue;
                                m_aChances[oChance.Currency] = oChance;
                                oChance.TradeTask = ChanceTask(oChance);
                            }
                            */
                            int nFound = 1;
                            if (m_aChancesCount.TryGetValue(oChance.Currency, out nFound))
                            {
                                nFound++;
                                if (nFound > 4) continue;
                            }
                            if( m_aChances.TryAdd(oChance.Currency, oChance) )
                            {
                                oChance.TradeTask = ChanceTask( oChance );
                                m_aChancesCount[oChance.Currency] = nFound;
                            }
                        }
                    }
                }
                await Task.Delay(200); 
            }
        }
    }
}

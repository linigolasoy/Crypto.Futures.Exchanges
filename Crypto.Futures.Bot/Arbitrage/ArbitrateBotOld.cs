using Crypto.Futures.Bot.Trading;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{
    /*
    /// <summary>
    /// Futures arbitrage between exchanges
    /// </summary>
    internal class ArbitrateBotOld: ITradingBot
    {

        private IFuturesExchange[]? m_aExchanges = null;
        private Task? m_oMainLoop = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private ConcurrentDictionary<string, ArbitrageChance> m_aChances = new ConcurrentDictionary<string, ArbitrageChance>();

        private List<IFuturesSymbol> m_aSubscribed = new List<IFuturesSymbol>();    
        public ArbitrateBotOld(IExchangeSetup oSetup, ICommonLogger oLogger, bool bPaperTrading )
        {
            Setup = oSetup;
            Logger = oLogger;
            Trader = new PaperTrader(this);
        }
        public IExchangeSetup Setup { get; }
        public ITrader Trader { get; }  
        public ICommonLogger Logger { get; }

        private DateTime m_dLastChanceFind = DateTime.Today;

        private decimal MINIMUM_PERCENT = 1.3M;
        private decimal m_nTotalProfit = 0;
        private ArbitrageChance? m_oActiveChance = null;
        /// <summary>
        /// Chance finding
        /// </summary>
        /// <returns></returns>
        private async Task FindChances()
        {
            DateTime dNow = DateTime.Now;
            if ((dNow - m_dLastChanceFind).TotalSeconds < 3) return;
            m_dLastChanceFind = dNow;
            if (m_aExchanges == null) return ;
            try
            {
                List<ITicker> aAllTickers = new List<ITicker>();

                List<Task<ITicker[]?>> aTasks = new List<Task<ITicker[]?>>();

                foreach (var oExchange in m_aExchanges)
                {
                    aTasks.Add(oExchange.Market.GetTickers());
                }

                await Task.WhenAll(aTasks);

                foreach (var oTask in aTasks)
                {
                    if (oTask.Result == null) continue;
                    aAllTickers.AddRange(oTask.Result);
                }

                Dictionary<string, List<ITicker>> oDictTickers = new Dictionary<string, List<ITicker>>();
                foreach (var oTicker in aAllTickers)
                {
                    if (oTicker.Symbol.Quote != "USDT") continue;
                    if (!oDictTickers.ContainsKey(oTicker.Symbol.Base))
                    {
                        oDictTickers.Add(oTicker.Symbol.Base, new List<ITicker>());
                    }
                    oDictTickers[oTicker.Symbol.Base].Add(oTicker);
                }

                // We might proceeed, we have it all
                ArbitrageChance? oBestChance = null;
                foreach (string strCurrency in oDictTickers.Keys)
                {
                    List<ITicker> aTickers = oDictTickers[strCurrency];
                    if (aTickers.Count < 2) continue;
                    for (int i = 0; i < aTickers.Count; i++)
                    {
                        ITicker oTicker1 = aTickers[i];
                        for (int j = i + 1; j < aTickers.Count; j++)
                        {
                            ITicker oTicker2 = aTickers[j];
                            ArbitrageChance? oActualChance = ArbitrageChance.Create(oTicker1, oTicker2);
                            if (oActualChance == null) continue;
                            if (oBestChance == null)
                            {
                                oBestChance = oActualChance;
                            }
                            else if (oBestChance.Percentage < oActualChance.Percentage)
                            {
                                oBestChance = oActualChance;
                            }

                        }
                    }

                }
                if (oBestChance != null)
                {
                    if (oBestChance.Percentage < MINIMUM_PERCENT) return;
                    string strCurrency = oBestChance.LongData.Symbol.Base;

                    ArbitrageChance? oFound = null;
                    if( m_aChances.TryGetValue(strCurrency, out oFound) )
                    {
                        if (oFound.Equals(oBestChance))
                        {
                            oFound.Update(oBestChance); 
                        }
                        else
                        {
                            m_aChances[strCurrency] = oBestChance;
                        }
                    }
                    else
                    {
                        m_aChances[strCurrency] = oBestChance;
                    }
                }
            }
            catch ( Exception ex ) 
            {
                Logger.Error("Error evaluating chances", ex);
            }
        }

        /// <summary>
        /// Subscribe to symbol if needed
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        private async Task<bool> DoSubscribe( IFuturesSymbol oSymbol )
        {
            IFuturesExchange oExchange = oSymbol.Exchange;
            if( m_aSubscribed.Any(p=> p.Exchange.ExchangeType == oExchange.ExchangeType && p.Symbol == oSymbol.Symbol) ) return false;
            if( !m_aSubscribed.Any(p=> p.Exchange.ExchangeType == oExchange.ExchangeType ) )
            {
                await oExchange.Market.Websocket.Start();
            }
            bool bResult = await oExchange.Market.Websocket.Subscribe(oSymbol);
            if( bResult ) m_aSubscribed.Add(oSymbol);   
            return bResult;
        }

        /// <summary>
        /// Buy check
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task CheckForBuy( ArbitrageChance oChance )
        {
            if (!oChance.IsDataValid) return;

            // Funding rate check
            DateTime dNow = DateTime.Now;
            if( (oChance.LongData.WsSymbolData!.FundingRate!.Next - dNow).TotalHours < 0.25 ) return;
            if ((oChance.ShortData.WsSymbolData!.FundingRate!.Next - dNow).TotalHours < 0.25) return;

            try
            {
                // Price check
                decimal nPriceLong = oChance.LongData.WsSymbolData!.LastTrade!.Price;
                decimal nPriceShort = oChance.ShortData.WsSymbolData!.LastTrade!.Price;
                decimal nDiff = nPriceShort - nPriceLong;
                if (nDiff <= 0) return;

                decimal nPercent = 100.0M * nDiff / nPriceLong;
                if (nPercent <= MINIMUM_PERCENT) return;
                oChance.Percentage = nPercent;

                decimal nQuantity = Trader.Money * Trader.Leverage / nPriceLong;
                int nDecimals = oChance.ShortData.Symbol.QuantityDecimals;
                if (oChance.LongData.Symbol.QuantityDecimals < oChance.ShortData.Symbol.QuantityDecimals)
                {
                    nDecimals = oChance.LongData.Symbol.QuantityDecimals;
                }
                nQuantity = Math.Round(nQuantity, nDecimals);

                List<Task<ITraderPosition?>> aTasks = new List<Task<ITraderPosition?>>();
                aTasks.Add(Trader.Open(oChance.LongData.Symbol, true, nQuantity));
                aTasks.Add(Trader.Open(oChance.ShortData.Symbol, false, nQuantity));

                await Task.WhenAll(aTasks);
                ITraderPosition? oPositionLong = null;  
                ITraderPosition? oPositionShort = null;
                foreach (var oTask in aTasks)
                {
                    if( oTask.Result == null ) continue;
                    if( oTask.Result.Symbol.Symbol == oChance.LongData.Symbol.Symbol && 
                        oTask.Result.Symbol.Exchange.ExchangeType == oChance.LongData.Symbol.Exchange.ExchangeType )
                    { 
                        oPositionLong = oTask.Result;
                    }
                    if (oTask.Result.Symbol.Symbol == oChance.ShortData.Symbol.Symbol &&
                        oTask.Result.Symbol.Exchange.ExchangeType == oChance.ShortData.Symbol.Exchange.ExchangeType)
                    {
                        oPositionShort = oTask.Result;
                    }
                }
                if( oPositionLong != null && oPositionShort != null )
                {
                    oChance.LongData.Position = oPositionLong;  
                    oChance.ShortData.Position = oPositionShort;

                    decimal nDiffReal = oChance.ShortData.Position.PriceOpen - oChance.LongData.Position.PriceOpen;
                    decimal nPercentReal = 100.0M * nDiffReal / oChance.LongData.Position.PriceOpen;
                    oChance.Percentage = nPercentReal;
                    oChance.PercentageClose = nPercentReal;
                    oChance.DateOpen = DateTime.Now;
                    Logger.Info($"====> Open position in {oChance.ToString()}");
                    m_oActiveChance = oChance;
                }
                // ITraderPosition? oPosition = await Trader.
                // Logger.Info( oChance.ToString() );  
            }
            catch (Exception ex)
            {
                Logger.Error("Trade error : ", ex);
            }

        }

        private async Task LoopChances()
        {
            foreach (var oChance in m_aChances.Values)
            {
                bool bSubscribed = false;
                // Subscribe check Long
                bSubscribed |= await DoSubscribe(oChance.LongData.Symbol);
                // Subscribe check Short
                bSubscribed |= await DoSubscribe(oChance.ShortData.Symbol);
                if (bSubscribed) continue;

                // Data update for long
                if( oChance.LongData.WsSymbolData == null )
                {
                    oChance.LongData.WsSymbolData = oChance.LongData.Symbol.Exchange.Market.Websocket.DataManager.GetData(oChance.LongData.Symbol);
                }
                // Data update for short
                if (oChance.ShortData.WsSymbolData == null)
                {
                    oChance.ShortData.WsSymbolData = oChance.ShortData.Symbol.Exchange.Market.Websocket.DataManager.GetData(oChance.ShortData.Symbol);

                }

                // Check if buy
                await CheckForBuy(oChance);
                if (m_oActiveChance != null) break;

            }
            return;
        }

        private void RemoveChance(ArbitrageChance oChance)
        {
            ArbitrageChance? oRemoved = null;
            m_aChances.TryRemove(oChance.LongData.Symbol.Base, out oRemoved);
        }


        /// <summary>
        /// Calculate percent desired
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private decimal CalculatePercentDesired(ArbitrageChance oChance)
        {
            if (oChance.DateOpen == null) return 0;
            if( oChance.PercentageClose < 0 ) return 0.01M;
            DateTime dNow = DateTime.Now;   
            double nMinutes = (dNow - oChance.DateOpen.Value).TotalMinutes;
            int nFraction = (int)(nMinutes / 20);
            decimal nPow = (decimal)Math.Pow(2, nFraction);
            return Math.Round( oChance.PercentageClose / nPow, 3);
        }
        private async Task ProcessActive()
        {
            if (m_oActiveChance == null) return;

            decimal nMoney = Trader.Money * Trader.Leverage;
            decimal nPercentDesired = CalculatePercentDesired(m_oActiveChance);
            decimal nDesired = (nPercentDesired * nMoney) / 100.0M;

            if (m_oActiveChance.LongData.Position == null) return;
            if (m_oActiveChance.ShortData.Position == null) return;

            m_oActiveChance.LongData.Position.Update();
            m_oActiveChance.ShortData.Position.Update();

            decimal nProfit = Math.Round( m_oActiveChance.LongData.Position.Profit - m_oActiveChance.ShortData.Position.Profit, 2);
            decimal nPercentProfit = Math.Round( 100.0M * nProfit / nMoney, 3);

            if (nProfit > nDesired)
            {
                List<Task<bool>> aCloseTasks = new List<Task<bool>>();
                aCloseTasks.Add(Trader.Close(m_oActiveChance.LongData.Position));
                aCloseTasks.Add(Trader.Close(m_oActiveChance.ShortData.Position));
                await Task.WhenAll(aCloseTasks);
                m_oActiveChance.Profit = (m_oActiveChance.LongData.Position.Profit - m_oActiveChance.ShortData.Position.Profit);
                Logger.Info($"====<  Closed {m_oActiveChance.ToString()} => Profit {Math.Round(m_oActiveChance.Profit.Value,2)}");
                m_nTotalProfit += m_oActiveChance.Profit.Value;

                Logger.Info($"[{Math.Round(m_nTotalProfit,2)}] TOTAL PROFIT");
                RemoveChance(m_oActiveChance);
                m_oActiveChance = null;
            }
            else
            {
                if( nProfit > m_oActiveChance.MaxProfit )
                {
                    m_oActiveChance.MaxProfit = nProfit;
                    Logger.Info($"     {m_oActiveChance.ToString()} => ({nPercentProfit}% / {nPercentDesired}%) Profit {nProfit}");
                }
                else
                {
                    DateTime dNow = DateTime.Now;
                    if( (dNow - m_oActiveChance.LastLog ).TotalMinutes > 1 )
                    {
                        m_oActiveChance.LastLog = dNow;
                        Logger.Info($"     {m_oActiveChance.ToString()} => ({nPercentProfit}% / {nPercentDesired}%) Profit {nProfit}");

                    }
                }
            }

            await Task.Delay(200);
        }

        private async Task MainLoop()
        {

            DateTime dLastLog = DateTime.Now;
            while (!m_oCancelSource.IsCancellationRequested)
            {
                if (m_aExchanges == null) continue;

                try
                {
                    List<Task> aTasks = new List<Task>();
                    if (m_oActiveChance == null)
                    {
                        aTasks.Add(FindChances());
                        aTasks.Add(LoopChances());
                    }
                    else
                    {
                        aTasks.Add(ProcessActive());    
                    }

                    await Task.WhenAll(aTasks);

                }
                catch (Exception ex)
                {
                    Logger.Error("ArbitrageBot. Error on main loop", ex);  
                }
                await Task.Delay(200);
            }
        }


        public async Task<bool> Start()
        {
            Logger.Info("ArbitrageBot starting...");
            if (m_oMainLoop != null)
            {
                await Stop();
            }

            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();

            foreach (var eType in Setup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(Setup, eType, Logger);
                aExchanges.Add(oExchange);
            }
            m_aExchanges = aExchanges.ToArray();
            m_oMainLoop = MainLoop();

            Logger.Info("ArbitrageBot started...");
            await Task.Delay(1000);
            return true;
        }

        public async Task<bool> Stop()
        {
            m_oCancelSource.Cancel();
            if (m_oMainLoop != null)
            {
                await m_oMainLoop;
                m_oMainLoop = null;
            }
            m_aExchanges = null;
            await Task.Delay(1000);
            Logger.Info("ArbitrageBot ended...");
            return true;
        }

    }
    */
}

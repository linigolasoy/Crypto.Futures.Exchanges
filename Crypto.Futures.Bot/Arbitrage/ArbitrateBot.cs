using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Arbitrage
{
    internal class ArbitrateBot: ITradingBot
    {
        private IFuturesExchange[]? m_aExchanges = null;
        private Task? m_oMainLoop = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private List<ArbitrageChance> m_aChances = new List<ArbitrageChance>(); 


        public ArbitrateBot(IExchangeSetup oSetup, ICommonLogger oLogger)
        {
            Setup = oSetup;
            Logger = oLogger;   
        }
        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }


        private async Task<ArbitrageChance?> FindChances()
        {
            if (m_aExchanges == null) return null;
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
                    if (oBestChance.Percentage < 1.0M) return null;
                    ArbitrageChance? oFound = m_aChances.FirstOrDefault(p =>
                                                        p.SymbolLong.Exchange.ExchangeType == oBestChance.SymbolLong.Exchange.ExchangeType &&
                                                        p.SymbolLong.Symbol == oBestChance.SymbolLong.Symbol &&
                                                        p.SymbolShort.Exchange.ExchangeType == oBestChance.SymbolShort.Exchange.ExchangeType &&
                                                        p.SymbolShort.Symbol == oBestChance.SymbolShort.Symbol);
                    if (oFound != null) oFound.Update(oBestChance);
                    else m_aChances.Add(oBestChance);
                }
                return oBestChance;
            }
            catch ( Exception ex ) 
            {
                Logger.Error("Error evaluating chances", ex);
                return null;    
            }
        }

        private async Task MainLoop()
        {

            DateTime dLastLog = DateTime.Now;
            while (!m_oCancelSource.IsCancellationRequested)
            {
                if (m_aExchanges == null) continue;

                ArbitrageChance? oChance = await FindChances();    
                if( oChance != null ) 
                {
                    Logger.Info(oChance.ToString()!);    
                }
                /*
                foreach (var oExchange in m_aExchanges)
                {
                    try
                    {
                        IFuturesSymbol[] aPrevious = oExchange.SymbolManager.GetAllValues();

                        IFuturesSymbol[]? aActual = await oExchange.RefreshSymbols();
                        if (aActual == null) continue;

                        IFuturesSymbol[] aNews = aActual.Where(p => !aPrevious.Any(q => p.Symbol == q.Symbol)).ToArray();
                        if (bFirst)
                        {
                            IFuturesSymbol[] aToday = aActual.Where(p => p.ListDate.Date == DateTime.Today).ToArray();
                            if (aToday.Length > 0)
                            {
                                foreach (var oSymbol in aToday)
                                {
                                    Logger.Info($"Today Symbol {oSymbol.ToString()} {oSymbol.ListDate.ToShortTimeString()}");
                                }
                            }
                        }

                        if (aNews == null || aNews.Length <= 0) continue;
                        foreach (var oSymbol in aNews)
                        {
                            Logger.Info($"New symbol {oSymbol.ToString()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error on exchange {oExchange.ExchangeType.ToString()}", ex);
                    }
                }
                bFirst = false;
                DateTime dNow = DateTime.Now;
                if ((dNow - dLastLog).TotalMinutes >= nMinutes)
                {
                    dLastLog = dNow;
                    Logger.Info("...Checking");
                }
                */
                await Task.Delay(3000);
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
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(Setup, eType);
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
}

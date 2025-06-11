using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot
{
    internal class NewSymbolBot : ITradingBot
    {
        private IFuturesExchange[]? m_aExchanges = null;
        private Task? m_oMainLoop = null;

        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();    

        public NewSymbolBot( IExchangeSetup oSetup, ICommonLogger oLogger) 
        { 
            Setup = oSetup;
            Logger = oLogger;
        }
        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }


        private async Task MainLoop()
        {

            DateTime dLastLog = DateTime.Now;
            double nMinutes = 5;
            bool bFirst = true; 
            while (!m_oCancelSource.IsCancellationRequested)
            {
                if (m_aExchanges == null) continue;
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
                            IFuturesSymbol[] aToday = aActual.Where(p=> p.ListDate.Date == DateTime.Today).ToArray();   
                            if (aToday.Length > 0 ) 
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
                if( (dNow - dLastLog ).TotalMinutes >= nMinutes )
                {
                    dLastLog = dNow;
                    Logger.Info("...Checking");
                }
                await Task.Delay(10000);
            }
        }
        public async Task<bool> Start()
        {
            Logger.Info("NewSymbolBot starting...");
            if( m_oMainLoop != null )
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
            
            Logger.Info("NewSymbolBot started...");
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
            Logger.Info("NewSymbolBot ended...");
            return true;
        }
    }
}

using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.Arbitrage;
using Crypto.Futures.Bot.Model.CryptoTrading;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{
    /// <summary>
    /// Arbitrage bot implementation
    /// </summary>
    internal class CryptoArbitrageBot : IArbitrageBot
    {

        internal CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private IArbitrageCurrencyFinder? m_oCurrencyFinder = null;
        private IArbitrageChanceEvaluator m_oEvaluator;
        private Task? m_oMainLoop = null;   

        public CryptoArbitrageBot( IExchangeSetup oSetup, bool bPaper = false) 
        { 
            Setup = oSetup; 
            Logger = ExchangeFactory.CreateLogger(Setup, this.GetType().Name);  
            // Create exchanges
            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            foreach (var eType in Setup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType, Logger);
                if( oExchange.Tradeable ) aExchanges.Add(oExchange);
            }
            Exchanges = aExchanges.ToArray();
            Trader = new CryptoTrader(this);
            m_oEvaluator = new ArbitrageChanceEvaluator(Setup);
            ChanceManager = new ArbitrageChanceManager(this);
        }
        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public IArbitrageChanceManager ChanceManager { get; }   

        public IFuturesExchange[] Exchanges { get; }
        public CancellationToken CancelToken { get => m_oCancelSource.Token; }  

        public ICryptoTrader Trader { get; }

        public async Task<bool> Close(string strCurrency)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CloseAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start bot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            Logger.Info("CryptoArbitrageBot Starting....");
            // Start private socket
            foreach(var oExchange in Exchanges)
            {
                bool bStarted = await oExchange.Account.WebsocketPrivate.Start();
                if( !bStarted )
                {
                    Logger.Error($"Could not start private websockets on {oExchange.ExchangeType.ToString()}");
                    return false;   
                }
            }
            // Delay for trader balance
            await Task.Delay(2000);
            Trader.InitBalances();  

            // Currency Finder
            m_oCurrencyFinder = new CryptoCurrencyFinder(this);
            bool bFinderStarted = await m_oCurrencyFinder.Start();
            if (!bFinderStarted)
            {
                Logger.Error($"Could not start CurrencyFinder");
                return false;
            }
            // Main loop
            m_oMainLoop = MainLoop();

            Logger.Info("CryptoArbitrageBot Started");
            return true;    
        }

        /// <summary>
        /// Ends bot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            Logger.Info("CryptoArbitrageBot Stopping....");
            // Stop private sockets
            foreach (var oExchange in Exchanges)
            {
                bool bStopped = await oExchange.Account.WebsocketPrivate.Stop();
                if (!bStopped)
                {
                    Logger.Warning($"Could not stop private websockets on {oExchange.ExchangeType.ToString()}");
                }
            }

            // Stop currency finder
            if( m_oCurrencyFinder != null)
            {
                await m_oCurrencyFinder.Stop(); 
                m_oCurrencyFinder = null;   
             }
            // Stop main loop
            if( m_oMainLoop != null)
            {
                m_oCancelSource.Cancel();
                await m_oMainLoop;
                m_oMainLoop = null;
                await Task.Delay(1000);
            }
            Logger.Info("CryptoArbitrageBot Stopped");
            return true;
        }


        /// <summary>
        /// Log profits
        /// </summary>
        private void LogProfits()
        {
            decimal nBalance = Math.Round( (Trader.Balances.Length <= 0 ? 0 : Trader.Balances.Select(p=> p.Balance).Sum()), 2); 
            decimal nProfitClosed = Math.Round( (Trader.PositionsClosed.Length <= 0 ? 0 : Trader.PositionsClosed.Select(p=> p.Profit).Sum()), 2);
            decimal nProfitOpen = Math.Round((Trader.PositionsActive.Length <= 0 ? 0: Trader.PositionsActive.Select(p => p.Profit).Sum()), 2);


            Logger.Info($">>>>> BALANCE = {nBalance} CLOSED = {nProfitClosed} OPEN = {nProfitOpen}");

        }

        /// <summary>
        /// Bot main loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            Logger.Info("CryptoArbitrageBot Main loop started");
            DateTime dNow = DateTime.Now;   
            DateTime dLastReport = new DateTime(dNow.Year, dNow.Month, dNow.Day, dNow.Hour, dNow.Minute,0, DateTimeKind.Local);
            while (!m_oCancelSource.IsCancellationRequested)
            {
                try
                {
                    if (m_oCurrencyFinder != null)
                    {
                        IArbitrageCurrency[] aCurrencies = m_oCurrencyFinder.Currencies;
                        if (aCurrencies.Length > 0)
                        {
                            IArbitrageChance[] aChances = m_oEvaluator.ToChances(aCurrencies);
                            if (aChances.Length > 0)
                            {
                                bool bAdded = await ChanceManager.Add(aChances); 
                            }
                        }
                    }
                    dNow = DateTime.Now;
                    double nDiff = (dNow - dLastReport).TotalMinutes;
                    if( nDiff >= 2 )
                    {
                        LogProfits();
                        dLastReport = dNow;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Main loop error",ex);
                }
                await Task.Delay(100);
            }
            Logger.Info("CryptoArbitrageBot Main loop ended");
        }
    }
}

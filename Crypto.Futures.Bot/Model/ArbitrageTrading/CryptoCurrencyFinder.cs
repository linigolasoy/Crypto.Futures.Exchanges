using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.Arbitrage;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{
    /// <summary>
    /// Finds possible arbitrages with tickers
    /// </summary>
    internal class CryptoCurrencyFinder : IArbitrageCurrencyFinder
    {

        private Task? m_oMainTask = null;
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();

        private ConcurrentDictionary<string, IArbitrageCurrency> m_aCurrencies = new ConcurrentDictionary<string, IArbitrageCurrency>();
        public CryptoCurrencyFinder( ICryptoBot oBot ) 
        { 
            Bot = oBot;
        }
        public ICryptoBot Bot { get; }

        public IArbitrageCurrency[] Currencies { get => m_aCurrencies.Values.ToArray(); }



        /// <summary>
        /// Get all tickers
        /// </summary>
        /// <returns></returns>
        private async Task<ITicker[]?> GetAllTickers()
        {
            try
            {
                List<Task<ITicker[]?>> aTasks = new List<Task<ITicker[]?>>();
                foreach( var oExchange in Bot.Exchanges)
                {
                    aTasks.Add(oExchange.Market.GetTickers());  
                }
                await Task.WhenAll( aTasks );
                List<ITicker> aResult = new List<ITicker>();    
                foreach (var oTask in aTasks)
                {
                    if (oTask.Result == null) continue;
                    aResult.AddRange(oTask.Result); 
                }
                return aResult.ToArray();   
            }
            catch (Exception ex)
            {
                Bot.Logger.Error("Error getting all exchange tickers", ex);
            }
            return null;
        }


        /// <summary>
        /// Tickers to dict
        /// </summary>
        /// <param name="aTickers"></param>
        /// <returns></returns>
        private Dictionary<string, ITicker[]> TickersToDict(ITicker[] aTickers)
        {
            Dictionary<string, List<ITicker>> aFound = new Dictionary<string, List<ITicker>>(); 
            foreach (var oTicker in aTickers)
            {
                if (oTicker.Symbol.Quote != "USDT") continue;

                if( !aFound.ContainsKey(oTicker.Symbol.Base))
                {
                    aFound.Add(oTicker.Symbol.Base, new List<ITicker>());   
                }
                aFound[oTicker.Symbol.Base].Add(oTicker);
            }

            Dictionary<string, ITicker[]> aResult = new Dictionary<string, ITicker[]>();
            foreach( var oDict in aFound)
            {
                if( oDict.Value.Count < 2) continue;
                aResult.Add(oDict.Key, oDict.Value.ToArray());  
            }
            return aResult;
        }


        /// <summary>
        /// Find pending
        /// </summary>
        /// <param name="oDictTickers"></param>
        /// <returns></returns>
        private IPendingChance[] FindPending(Dictionary<string, ITicker[]> oDictTickers)
        {
            List<IPendingChance> aResult = new List<IPendingChance>();
            foreach( var oKeyValuePair in oDictTickers)
            {
                if (m_aCurrencies.ContainsKey(oKeyValuePair.Key)) continue;
                ITicker oMax = oKeyValuePair.Value.OrderByDescending(p => p.LastPrice).First();
                ITicker oMin = oKeyValuePair.Value.OrderBy(p => p.LastPrice).First();
                if (oMax.Symbol.Exchange.ExchangeType == oMin.Symbol.Exchange.ExchangeType) continue;
                if( oMin.LastPrice <= 0 || oMax.LastPrice <= 0 ) continue;  
                decimal nPercent = 100.0M * (oMax.LastPrice - oMin.LastPrice) / oMin.LastPrice;
                if (nPercent >= 10.0M || nPercent < Bot.Setup.Arbitrage.MinimumPercent) continue;
                aResult.Add( new CurrencyPendingChance(oKeyValuePair.Key, oKeyValuePair.Value.Select(p=> p.Symbol).ToArray(), nPercent) );
            }
            return aResult.ToArray();   
        }

        /// <summary>
        /// Set leverage of all symbols on a chance
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task<bool> SetLeverage( IPendingChance oChance )
        {
            List<Task<bool>> aTasks = new List<Task<bool>>();   
            foreach( var oSymbol in oChance.Symbols )
            {
                aTasks.Add( Bot.Trader.PutLeverage(oSymbol) );
            }
            await Task.WhenAll( aTasks.ToArray() );
            if( aTasks.Any(p=> !p.Result) ) return false;
            await Task.Delay(500);
            return true;
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task DoSubscribe(IPendingChance oChance)
        {
            List<Task<IWebsocketSubscription?>> aTasks = new List<Task<IWebsocketSubscription?>>();
            foreach( var oSymbol in oChance.Symbols )
            {
                aTasks.Add( oSymbol.Exchange.Market.Websocket.Subscribe(oSymbol, WsMessageType.OrderbookPrice));
            }
            await Task.WhenAll(aTasks);
            List<IWebsocketSubscription> aSubscribed = new List<IWebsocketSubscription>();
            foreach( var oTask in aTasks)
            {
                if( oTask.Result == null ) continue;    
                aSubscribed.Add( oTask.Result );
            }
            int nSeconds = 20;
            int nDelay = 100;
            List<IOrderbookPrice> aFound = new List<IOrderbookPrice>();
            int nRetries = nSeconds * 1000 / nDelay;
            while( nRetries > 0 )
            {
                foreach (var oSubscription in aSubscribed)
                {
                    if( aFound.Any(p=> p.Symbol.Symbol == oSubscription.Symbol.Symbol && p.Symbol.Exchange.ExchangeType == oSubscription.Symbol.Exchange.ExchangeType))
                    {
                        continue;
                    }
                    var oData = oSubscription.Symbol.Exchange.Market.Websocket.DataManager.GetData(oSubscription.Symbol);
                    if( oData == null ) continue;
                    if( oData.LastOrderbookPrice == null ) continue;
                    aFound.Add(oData.LastOrderbookPrice );
                }
                if (aFound.Count >= aSubscribed.Count) break;
                nRetries--;
                await Task.Delay( nDelay );
            }
            // Add to subscribed
            if (aFound.Count < 2) return;
            m_aCurrencies.TryAdd( oChance.Currency, new ArbitrageCurrency(oChance.Currency, aFound.ToArray()) ); 
        }


        /// <summary>
        /// Main looop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {

            DateTime dLastLog = DateTime.Now;
            // Main loop
            while (!m_oCancelSource.IsCancellationRequested)
            {
                try
                {
                    // TODO: Get all tickers
                    ITicker[]? aTickers = await GetAllTickers();
                    if (aTickers != null)
                    {
                        // Tickers to Dict
                        Dictionary<string, ITicker[]> oDictTickers = TickersToDict(aTickers);
                        // Get pending
                        IPendingChance[] aPending = FindPending(oDictTickers);
                        foreach (var oPending in aPending)
                        {
                            // Bot.Logger.Info($"  Possible chance on {oPending.ToString()}");
                            // Pending leverages
                            bool bLeverage = await SetLeverage(oPending);
                            if (!bLeverage) continue;
                            // Subscribe to pending and add to dict
                            await DoSubscribe(oPending);
                        }
                    }

                    DateTime dNow = DateTime.Now;
                    double nMinutes = (dNow - dLastLog).TotalMinutes;
                    if( nMinutes >= 1)
                    {
                        dLastLog = dNow;
                        Bot.Logger.Info($"   CryptoCurrencyFinder. Currency count = {m_aCurrencies.Count}");
                    }

                }
                catch (Exception ex)
                {
                    Bot.Logger.Error("CryptoCurrencyFinder: Mainloop error", ex);
                }
                await Task.Delay(1000);
            }
        }
        /// <summary>
        /// Start chance finder
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            Bot.Logger.Info("CryptoCurrencyFinder starting...");
            m_oCancelSource = new CancellationTokenSource();
            m_oMainTask = MainLoop();
            await Task.Delay(1000);
            Bot.Logger.Info("CryptoCurrencyFinder started");
            return true;
        }

        /// <summary>
        /// Stop chance finder
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            Bot.Logger.Info("CryptoCurrencyFinder stopping...");
            if (m_oMainTask != null)
            {
                m_oCancelSource.Cancel();
                await m_oMainTask;
                m_oMainTask = null;
            }
            Bot.Logger.Info("CryptoCurrencyFinder stopped");
            await Task.Delay(1000);
            return true;
        }
    }
}

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

namespace Crypto.Futures.Bot.Arbitrage.Model
{

    /// <summary>
    /// Find chances by constantly querying tickers
    /// </summary>
    internal class ArbitrageFinder : IArbitrageFinder
    {
        private ConcurrentDictionary<string, IWebsocketSymbolData[]> m_aSubscribed = new ConcurrentDictionary<string, IWebsocketSymbolData[]>();
        private CancellationTokenSource m_oCancelSource = new CancellationTokenSource();
        private Task? m_oMainTask = null;

        public ArbitrageFinder( ITradingBot oBot ) 
        { 
            Bot = oBot;
        }
        public ITradingBot Bot { get; }

        public string[] SubscribedCurrencies { get => m_aSubscribed.Keys.ToArray(); }

        /// <summary>
        /// Create exchanges
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IFuturesExchange[]? CreateExchanges()
        {
            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();

            foreach (var eType in Bot.Setup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(Bot.Setup, eType, Bot.Logger);
                if (!oExchange.Tradeable) continue;
                aExchanges.Add(oExchange);
            }
            return aExchanges.ToArray();
        }



        /// <summary>
        /// Get list of all tickers
        /// </summary>
        /// <param name="aExchanges"></param>
        /// <returns></returns>
        private async Task<ITicker[]?> GetAllTickers(IFuturesExchange[] aExchanges)
        {
            List<ITicker> aAllTickers = new List<ITicker>();

            List<Task<ITicker[]?>> aTasks = new List<Task<ITicker[]?>>();

            foreach (var oExchange in aExchanges)
            {
                aTasks.Add(oExchange.Market.GetTickers());
            }

            await Task.WhenAll(aTasks);

            foreach (var oTask in aTasks)
            {
                if (oTask.Result == null) continue;
                aAllTickers.AddRange(oTask.Result);
            }
            return aAllTickers.ToArray();   

        }
        /// <summary>
        /// Find pending subscriptions
        /// </summary>
        /// <param name="aEchanges"></param>
        /// <param name="aPairs"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<Dictionary<string, ITicker[]>?> FindPendingSubscriptions(IFuturesExchange[] aExchanges)
        {
            // Get all tickers from all echanges
            ITicker[]? aAllTickers = await GetAllTickers(aExchanges);
            if( aAllTickers == null ) return null;
            // Get pairs tickers
            Dictionary<string, List<ITicker>> aPreFind = new Dictionary<string, List<ITicker>>();
            foreach( var oTicker in aAllTickers )
            {
                if( oTicker.Symbol.Quote != "USDT") continue; 
                if( !aPreFind.ContainsKey(oTicker.Symbol.Base) ) aPreFind.Add(oTicker.Symbol.Base, new List<ITicker>());
                aPreFind[oTicker.Symbol.Base].Add(oTicker); 
            }


            Dictionary<string, ITicker[]> aResult = new Dictionary<string, ITicker[]>();    
            foreach ( var oPre in aPreFind )
            {
                if (SubscribedCurrencies.Contains(oPre.Key)) continue;
                if( oPre.Value.Count < 2 ) continue;
                // Check if possible chance
                decimal nMin = oPre.Value.Where(p=> p.LastPrice > 0).Min(p => p.LastPrice);
                decimal nMax = oPre.Value.Where(p => p.LastPrice > 0).Max(p => p.LastPrice);
                decimal nDiff = nMax - nMin;
                decimal nPercent = Math.Round( 100.0M * nDiff / nMin, 2);
                if (nPercent < Bot.Setup.Arbitrage.MinimumPercent) continue;
                if( nPercent > 10.0M ) continue;    
                Bot.Logger.Info($"   Possible chance on {oPre.Key} => {nPercent}");
                aResult.Add(oPre.Key, oPre.Value.ToArray());    
            }

            return aResult;
        }

        /// <summary>
        /// Subscribe to pending
        /// </summary>
        /// <param name="aKeys"></param>
        /// <param name="aPairs"></param>
        /// <returns></returns>
        private async Task<bool> SubscribePending(Dictionary<string, ITicker[]> aPending)
        {
            if (aPending.Count <= 0) return true;
            List<ITicker> aAllPending = new List<ITicker>();
            foreach (var item in aPending)
            {
                aAllPending.AddRange(item.Value);
            }
            ExchangeType[] aTypes = aAllPending.Select(p=> p.Symbol.Exchange.ExchangeType).Distinct().ToArray();


            // Subscribe to symbols on each exchange
            foreach (var eType in aTypes)
            {
                IFuturesSymbol[] aPendingSymbols = aAllPending.Where(p => p.Symbol.Exchange.ExchangeType == eType).Select(p=> p.Symbol).ToArray();
                if( aPendingSymbols.Length <= 0 ) continue;
                IFuturesExchange oExchange = aPendingSymbols[0].Exchange;
                // Start websockets
                if( !oExchange.Market.Websocket.Started )
                {
                    Bot.Logger.Info($"   Starting websocket on {eType.ToString()}...");
                    bool bRes = await oExchange.Market.Websocket.Start();
                    if (!bRes) continue;
                }

                // Start websockets
                await oExchange.Market.Websocket.Subscribe(aPendingSymbols);

            }


            // Wait for gather symbol data
            foreach( var oPending in aPending)
            {
                while( true )
                {
                    List<IWebsocketSymbolData> aSymbolData = new List<IWebsocketSymbolData>();  
                    foreach( var oTicker in oPending.Value )
                    {
                        IWebsocketSymbolData? oFound = oTicker.Symbol.Exchange.Market.Websocket.DataManager.GetData(oTicker.Symbol);
                        if (oFound == null) break;
                        aSymbolData.Add(oFound);
                    }
                    if( aSymbolData.Count >= oPending.Value.Length )
                    {
                        // DUDU
                        m_aSubscribed.TryAdd( oPending.Key, aSymbolData.ToArray() );
                        break;
                    }
                    else await Task.Delay(200);
                }
            }

            return true;
        }
        /// <summary>
        /// Main loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            // Create exchanges
            IFuturesExchange[]? aExchanges = CreateExchanges();
            if (aExchanges == null) return;
            // Create currency pairs
            while(!m_oCancelSource.IsCancellationRequested)
            {
                try
                {
                    Dictionary<string, ITicker[]>? aPending = await FindPendingSubscriptions(aExchanges);
                    if (aPending != null)
                    {
                        bool bSubscribed = await SubscribePending(aPending);
                        if (!bSubscribed) continue;
                    }
                }
                catch (Exception e)
                {
                    Bot.Logger.Error("ArbitrageFinder. MainLoop error", e);
                }
                await Task.Delay(2000);
            }
        }

        /// <summary>
        /// Check for valid chances
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IArbitrageChance[]?> FindValidChances()
        {
            try
            {
                List<IArbitrageChance> aResult = new List<IArbitrageChance>();  
                foreach (string strKey in SubscribedCurrencies)
                {
                    IWebsocketSymbolData[]? aData = null;
                    if (m_aSubscribed.TryGetValue(strKey, out aData))
                    {
                        // Find minimum and maximum
                        IWebsocketSymbolData? oMin = aData.Where(p=> p.FundingRate != null && p.LastOrderbookPrice != null ).OrderBy(p=> p.LastOrderbookPrice!.AskPrice).FirstOrDefault();
                        IWebsocketSymbolData? oMax = aData.Where(p => p.FundingRate != null && p.LastOrderbookPrice != null).OrderByDescending(p => p.LastOrderbookPrice!.BidPrice).FirstOrDefault();
                        if (oMin == null || oMax == null) continue;
                        if (oMin.Symbol.Exchange.ExchangeType == oMax.Symbol.Exchange.ExchangeType) continue;

                        decimal nPriceMinAsk = oMin.LastOrderbookPrice!.AskPrice;
                        decimal nPriceMinBid = oMin.LastOrderbookPrice!.BidPrice;
                        if (nPriceMinAsk <= 0 || nPriceMinBid <= 0) continue;
                        decimal nPriceMaxAsk = oMax.LastOrderbookPrice!.AskPrice;
                        decimal nPriceMaxBid = oMax.LastOrderbookPrice!.BidPrice;
                        if (nPriceMaxAsk <= 0 || nPriceMaxBid <= 0) continue;
                        decimal nPercentMin = Math.Round(100.0M * (nPriceMinAsk - nPriceMinBid) / nPriceMinBid, 2);
                        decimal nPercentMax = Math.Round(100.0M * (nPriceMaxAsk - nPriceMaxBid) / nPriceMaxBid, 2);
                        decimal nDiff = nPriceMaxBid - nPriceMinAsk;
                        decimal nPercent = Math.Round( nDiff * 100.0M / nPriceMinAsk, 2);
                        nPercent -= nPercentMin;
                        nPercent -= nPercentMax;    
                        if ( nPercent >= 10.0M || nPercent < Bot.Setup.Arbitrage.MinimumPercent ) continue;
                        aResult.Add( new ArbitrageChanceModel(this, strKey, nPercent, oMin, oMax) );
                    }
                }
                return aResult.ToArray();
            }
            catch (Exception e)
            {
                Bot.Logger.Error("Error finding valid chances", e);
            }
            return null;
        }

        /// <summary>
        /// Starts finder
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            if (m_oMainTask != null)
            {
               await Stop();    
            }
            m_oCancelSource = new CancellationTokenSource();
            m_oMainTask = MainLoop();
            return true;
        }

        /// <summary>
        /// Ends finder
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            if( m_oMainTask != null)
            {
                m_oCancelSource.Cancel();
                await m_oMainTask;
                m_oMainTask = null;
            }
            return true;
        }
    }
}

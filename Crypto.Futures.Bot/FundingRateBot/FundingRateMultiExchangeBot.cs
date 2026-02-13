using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Bot.Model.CryptoTrading;
using Crypto.Futures.Bot.Trading;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;

namespace Crypto.Futures.Bot.FundingRateBot
{
    internal class FundingRateMultiExchangeBot : IFundingRateBot
    {
        private const int MAX_ACTIVE = 1;

        private DateTime m_dLastProfitUpdate = DateTime.MinValue;   
        private CancellationTokenSource m_oCts = new CancellationTokenSource();

        private Task? m_oMainTask = null;

        private List<IFundingRateChance> m_aActiveChances = new List<IFundingRateChance>();
        private List<IFundingRateChance> m_aClosedChances = new List<IFundingRateChance>();

        private IFuturesExchange[] m_aExchanges = Array.Empty<IFuturesExchange>();

        public FundingRateMultiExchangeBot(IExchangeSetup oSetup, ICommonLogger oLogger, bool bPaperTrading)
        {
            Setup = oSetup;
            Logger = oLogger;
            List<IFuturesExchange> aEchanges = new List<IFuturesExchange>();
            ChanceFinder = new FundingChanceFinder(this);
        }
        public IFundingRateChance[] Chances { get => m_aActiveChances.ToArray(); }

        public IFundingChanceFinder ChanceFinder { get; }
        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public bool MarkToClose { get; set; } = false;
        public IFuturesExchange[] Exchanges { get=> m_aExchanges; }

        public ICryptoTrader? Trader { get; private set; } = null;

        private IAccountWatcher? Watcher { get; set; } = null;
        private IQuoter? Quoter { get; set; } = null;   

        public CancellationToken CancelToken { get => m_oCts.Token; }


        /// <summary>
        /// Find new funding rate chances
        /// </summary>
        /// <returns></returns>
        private async Task FindNewChances()
        {
            if (Trader == null) return;
            int nActive = m_aActiveChances.Where(p => p.IsActive).Count();
            if( nActive >= MAX_ACTIVE || MarkToClose )
            {
                return;
            }

            var aChances = await ChanceFinder.FindNewChances();
            if (aChances == null || aChances.Length <= 0) return;
            foreach( var oChance in aChances.OrderByDescending(p=> p.PercentDifference) )
            {
                IFundingRateChance[] aFound = m_aActiveChances.Where(p => p.Currency == oChance.Currency).ToArray();
                if (aFound.Length > 0)
                {
                    Logger.Info($"FundingRateMultiExchangeBot.FindNewChances: Chance for {oChance.Currency} already active");
                    continue;
                }
                decimal? nPriceLong = await Trader.Quoter.GetLongPrice(oChance.SymbolLong.Symbol);
                decimal? nPriceShort = await Trader.Quoter.GetShortPrice(oChance.SymbolShort.Symbol);
                if(nPriceLong == null || nPriceShort == null)
                {
                    Logger.Info($"FundingRateMultiExchangeBot.FindNewChances: Cannot get price for {oChance.Currency}, skipping chance");
                    continue;
                }

                m_aActiveChances.Add(oChance);
                if (m_aActiveChances.Count >= MAX_ACTIVE) break;
            }
        }


        /// <summary>
        /// Open positions for the given chance
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task OpenPositions( IFundingRateChance oChance )
        {
            decimal? nPriceLong = await Trader!.Quoter.GetLongPrice(oChance.SymbolLong.Symbol);
            decimal? nPriceShort = await Trader!.Quoter.GetShortPrice(oChance.SymbolShort.Symbol);
            if (nPriceLong == null || nPriceShort == null) return;
            // if( nPriceLong > nPriceShort ) return; // No chance if long price is higher than short price
            decimal nPriceMax = Math.Max(nPriceLong.Value, nPriceShort.Value);
            decimal nMoney = Trader.Money * Trader.Leverage;
            decimal? nQuantity = Trader!.Quoter.GetBestQuantity(new IFuturesSymbol[] { oChance.SymbolLong.Symbol, oChance.SymbolShort.Symbol }, nPriceMax, nMoney);
            if (nQuantity == null || nQuantity <= 0) return;
            Task<IPosition?> oTaskLong = Trader.Open(oChance.SymbolLong.Symbol, true, nQuantity.Value);
            Task<IPosition?> oTaskShort = Trader.Open(oChance.SymbolShort.Symbol, false, nQuantity.Value);

            await Task.WhenAll(oTaskLong, oTaskShort);
            IPosition? oPositionLong = oTaskLong.Result;
            IPosition? oPositionShort = oTaskShort.Result;

            oChance.SymbolLong.Position = oPositionLong;
            oChance.SymbolShort.Position = oPositionShort;

            return;
            /*
            decimal nMoney = Setup.MoneyDefinition.Money * Setup.MoneyDefinition.Leverage;
            if (oChance.SymbolLong.PriceOpen == null)
            {
                // B(var oPosition = await Trader.Open(oChance.SymbolLong.Symbol, Setup.MoneyDefinition.Money);
            }
            if (oChance.SymbolShort.PriceOpen == null)
            {

            }
            */
        }

        /*
        /// <summary>
        /// Update next funding times for the given symbol data if needed
        /// </summary>
        /// <param name="oSymbolData"></param>
        /// <returns></returns>
        private async Task<bool> UpdateSymbolData(IFundingRateSymbolData oSymbolData)
        {
            if (oSymbolData.RateOpen.Next <= DateTime.Now)
            {
                var oNewRate = await oSymbolData.Symbol.Exchange.Market.GetFundingRate(oSymbolData.Symbol);
                if (oNewRate != null) oSymbolData.RateOpen = oNewRate;
            }
        }
        */

        private async Task<IFundingRate?> GetUpdatedFunding(IFuturesSymbol oSymbol)
        {
            try
            {
                IFundingRate[]? aRates = await oSymbol.Exchange.Market.GetFundingRates();
                if (aRates == null || aRates.Length == 0) return null;
                IFundingRate[] aRatas = aRates.Where(p => p.Symbol.Base == oSymbol.Base).ToArray();
                IFundingRate? oFound = aRates.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol); 
                return oFound;
            }
            catch( Exception ex )
            {
                Logger.Error($"FundingRateMultiExchangeBot.GetUpdatedFunding: Error getting funding for symbol {oSymbol.Symbol}: {ex.Message}"); 
                return null;
            }
        }


        private async Task CloseChance(IFundingRateChance oChance)
        {
            try
            {
                if( Trader == null) return;
                if (oChance.SymbolLong.Position == null || oChance.SymbolShort.Position == null) return;
                Task<bool> oTaskCloseLong = Trader.Close(oChance.SymbolLong.Position);
                Task<bool> oTaskCloseShort = Trader.Close(oChance.SymbolShort.Position);
                await Task.WhenAll(oTaskCloseLong, oTaskCloseLong);
                bool bClosedLong = oTaskCloseLong.Result; 
                bool bClosedShort = oTaskCloseShort.Result;
                await Task.Delay(2000);
                if (bClosedLong && bClosedShort) 
                {
                    if( oChance.SymbolLong.Position.IsOpen || oChance.SymbolShort.Position.IsOpen )
                    {
                        Logger.Info($"FundingRateMultiExchangeBot.CloseChance: Chance for {oChance.Currency} closed successfully");
                    }
                    else
                    {
                        Logger.Error($"FundingRateMultiExchangeBot.CloseChance: Chance for {oChance.Currency} closed but positions still open");
                    }
                }
                ((FundingRateChance)oChance).IsActive = false;
                oChance.NeedClose = false;

            }
            catch (Exception ex) 
            { 
                Logger.Error($"FundingRateMultiExchangeBot.CloseChance: Error closing chance for {oChance.Currency}: {ex.Message}"); 
            }
        }
        /// <summary>
        /// Try to close positions for the given chance
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task TryClosePositions(IFundingRateChance oChance)
        {
            DateTime dMin = (oChance.SymbolLong.RateOpen.Next > oChance.SymbolShort.RateOpen.Next ? oChance.SymbolShort.RateOpen.Next : oChance.SymbolLong.RateOpen.Next);
            DateTime dNow = DateTime.Now;

            if( oChance.NeedClose )
            {
                await CloseChance(oChance);
                return;
            }


            // If next funding date is in the past, update next funding times


            // Check if needs to update funding
            double nMinutesUpdate = (dNow - oChance.LastFundingUpdate).TotalMinutes;
            if (nMinutesUpdate > 2 || dMin < dNow)
            {
                IFundingRate? oFundingLong = await GetUpdatedFunding(oChance.SymbolLong.Symbol);
                if (oFundingLong == null) return;
                IFundingRate? oFundingShort = await GetUpdatedFunding(oChance.SymbolShort.Symbol); 
                if (oFundingShort == null) return; 
                oChance.SymbolLong.RateOpen = oFundingLong; 
                oChance.SymbolShort.RateOpen = oFundingShort; 
                oChance.LastFundingUpdate = DateTime.Now;
                if( dMin < dNow )
                {
                    double nMinutes = (dNow - dMin).TotalMinutes;
                    if (nMinutes < 2) return;
                    ((FundingRateChance)oChance).ChanceNextFundingDate  = (oChance.SymbolLong.RateOpen.Next > oChance.SymbolShort.RateOpen.Next ? oChance.SymbolShort.RateOpen.Next : oChance.SymbolLong.RateOpen.Next);
                    dMin = oChance.ChanceNextFundingDate;
                }
            }



            decimal nFundingLong = 0;
            decimal nFundingShort = 0;

            if (oChance.SymbolLong.RateOpen.Next == dMin) nFundingLong = -oChance.SymbolLong.RateOpen.Rate;
            if (oChance.SymbolShort.RateOpen.Next == dMin) nFundingShort = oChance.SymbolShort.RateOpen.Rate;

            decimal nFundingTot = nFundingLong + nFundingShort;
            bool bUpdatedPnl = false;
            if (Trader != null)
            {
                Task<decimal?> oTaskProfitLong = Trader.Quoter.GetProfit(oChance.SymbolLong.Position!);
                Task<decimal?> oTaskProfitShort = Trader.Quoter.GetProfit(oChance.SymbolShort.Position!);
                await Task.WhenAll(oTaskProfitLong, oTaskProfitShort);
                decimal? nProfitLong = oTaskProfitLong.Result;
                decimal? nProfitShort = oTaskProfitShort.Result;
                if( nProfitLong != null && nProfitShort != null )
                {
                    bUpdatedPnl = true;
                    decimal nProfitTot = nProfitLong.Value + nProfitShort.Value;
                    oChance.Pnl = nProfitTot;
                }
            }

            if (nFundingTot > 0)
            {
                ((FundingRateChance)oChance).PercentDifference = 100M * nFundingTot;
                return;
            }
            if( bUpdatedPnl )
            {
                decimal nPercent = 100M * oChance.Pnl / Setup.MoneyDefinition.Money;
                if( nPercent > -1M )
                {
                    await CloseChance(oChance);
                }
            }
            return;

        }


        /// <summary>
        /// Get funding rate for the given position
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="aFundings"></param>
        /// <returns></returns>
        private async Task<IFundingRate?> GetPositionFunding(IPosition oPosition, Dictionary<ExchangeType, IFundingRate[]> aFundings)
        {
            IFundingRate[]? aRates = null;
            if(aFundings.ContainsKey(oPosition.Symbol.Exchange.ExchangeType))
            {
                aRates = aFundings[oPosition.Symbol.Exchange.ExchangeType];
            }

            if( aRates == null )
            {
                aRates = await oPosition.Symbol.Exchange.Market.GetFundingRates();
            }
            if(aRates == null || aRates.Length == 0) return null;
            aFundings[oPosition.Symbol.Exchange.ExchangeType] = aRates;

            IFundingRate? oFound = aRates.FirstOrDefault(p => p.Symbol.Symbol == oPosition.Symbol.Symbol);
            return oFound;
        }

        /// <summary>
        /// Find chances that are open on bot start and try to close them if needed
        /// </summary>
        /// <returns></returns>
        private async Task FindPreviousChances()
        {
            Logger.Info("FundingRateMultiExchangeBot.MainLoop: Find previous chances");
            Dictionary<string, List<IPosition>> oDictPositions = new Dictionary<string, List<IPosition>>();
            foreach( var oExchange in this.Exchanges )
            {
                var aPositions = await oExchange.Account.GetPositions();
                if (aPositions == null || aPositions.Length == 0) continue;
                foreach (var oPosition in aPositions)
                {
                    if (oPosition.Symbol.Quote != "USDT") continue;
                    if (!oDictPositions.ContainsKey(oPosition.Symbol.Base)) oDictPositions[oPosition.Symbol.Base] = new List<IPosition>();
                    oDictPositions[oPosition.Symbol.Base].Add(oPosition);
                }
            }

            Dictionary<ExchangeType, IFundingRate[]> aFundings = new Dictionary<ExchangeType, IFundingRate[]>();    
            foreach (var oItem in oDictPositions)
            {
                string strCurrency = oItem.Key;
                var aPositions = oItem.Value;
                if (aPositions.Count != 2) continue;
                IPosition? oPositionLong = aPositions.FirstOrDefault(p => p.IsLong );
                IPosition? oPositionShort = aPositions.FirstOrDefault(p => !p.IsLong);
                if(oPositionLong == null || oPositionShort == null) continue;
                if (oPositionLong.Quantity != oPositionShort.Quantity ) continue;

                IFundingRate? oFundingLong = await GetPositionFunding(oPositionLong, aFundings);
                IFundingRate? oFundingShort = await GetPositionFunding(oPositionShort, aFundings);
                if( oFundingLong == null || oFundingShort == null ) continue;
                DateTime dMin = (oFundingShort.Next > oFundingLong.Next ? oFundingLong.Next : oFundingShort.Next);

                decimal nDiffFunding = 0;
                if (oFundingLong.Next == dMin) nDiffFunding += oFundingLong.Rate;
                if (oFundingShort.Next == dMin) nDiffFunding += oFundingShort.Rate;
                nDiffFunding = Math.Abs(nDiffFunding) * 100M;

                IFundingRateChance oChance = new FundingRateChance(this, oFundingLong, oFundingShort, nDiffFunding);
                oChance.SymbolLong.Position = oPositionLong;
                oChance.SymbolShort.Position = oPositionShort;
                m_aActiveChances.Add(oChance);

            }

        }


        /// <summary>
        /// Display profits
        /// </summary>
        private void DisplayProfits()
        {
            double nSeconds = (DateTime.Now - m_dLastProfitUpdate).TotalSeconds; 
            
            
            if (nSeconds < 30) return; 
            
            m_dLastProfitUpdate = DateTime.Now; 
            decimal nTotalProfit = 0;

            foreach (var oChance in m_aActiveChances)
            {
                if (oChance.SymbolLong.Position == null || oChance.SymbolShort.Position == null) continue;
                nTotalProfit += oChance.Pnl;
            }
            Logger.Info($"FundingRateMultiExchangeBot.DisplayProfits: Total profit: {nTotalProfit}");
        }


        /// <summary>
        /// Main loop   
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            await FindPreviousChances();
            Logger.Info("FundingRateMultiExchangeBot.MainLoop: Starting main loop");

            while (!m_oCts.IsCancellationRequested)
            {
                try
                {
                    await FindNewChances();
                    bool bMarkToClose = MarkToClose;
                    MarkToClose = false; // Reset mark to close after checking, so it needs to be set again if needed in the next loop
                    foreach ( var oChance in m_aActiveChances)
                    {
                        if( !oChance.IsActive ) continue;
                        if ( oChance.SymbolLong.Position == null && oChance.SymbolShort.Position == null )
                        {
                            await OpenPositions(oChance);
                            continue;
                        }
                        else
                        {
                            if (bMarkToClose) oChance.NeedClose = true;
                            await TryClosePositions(oChance);
                            
                        }
                    }
                    DisplayProfits();
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    Logger?.Error($"FundingRateMultiExchangeBot.MainLoop: {ex.Message}");
                }
            }
            Logger!.Info("FundingRateMultiExchangeBot.MainLoop: Ending main loop");
            await Task.Delay(1000);

        }

        /// <summary>
        /// Start closing all positions and cancel all orders for the given currency
        /// </summary>
        /// <param name="strCurrency"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Close(string strCurrency)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Start closing all positions and cancel all orders   
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> CloseAll()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Find exchanges to use
        /// </summary>
        /// <returns></returns>
        private async Task<bool> FindExchanges()
        {
            ExchangeType[] aExchangeTry = new ExchangeType[]
            {
                ExchangeType.Hyperliquidity,
                ExchangeType.BingxFutures,
                ExchangeType.BitgetFutures,
                // ExchangeType.BitmartFutures,
                ExchangeType.CoinExFutures
            };

            List<IFuturesExchange> aEchanges = new List<IFuturesExchange>();
            foreach (var item in aExchangeTry)
            {
                IFuturesExchange oNew = ExchangeFactory.CreateExchange(Setup, item, Logger);
                await Task.Delay(500);
                IBalance[]? aBalances = await oNew.Account.GetBalances();
                if (aBalances == null || aBalances.Length == 0) continue;
                IBalance? oFound = aBalances.FirstOrDefault(p => p.Currency == "USDT");
                if (oFound == null || oFound.Balance <= 0) continue;
                if (oFound.Avaliable + oFound.Locked < Setup.MoneyDefinition.Money/2) continue;
                aEchanges.Add(oNew);
            }

            if (aEchanges.Count < 2) return false;

            m_aExchanges = aEchanges.ToArray();
            return true;

        }

        /// <summary>
        /// Start the bot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            if (m_oMainTask != null) await Stop();
            bool bFound = await FindExchanges();
            if (!bFound)
            {
                Logger.Error("FundingRateMultiExchangeBot.Start: No exchanges found, cannot start bot");
                return false;
            }
            // Create quoter and account watcher
            Quoter = new CryptoQuoter(this.Exchanges);
            Watcher = new CryptoAccountWatcher(this.Exchanges);

            await Watcher.Start();

            Trader = BotFactory.CreateTrader(this.Setup, this.Logger, this.Watcher, this.Quoter);
            if(Trader == null)
            {
                Logger.Error("FundingRateMultiExchangeBot.Start: Failed to create trader");
                return false;
            }


            m_oCts = new CancellationTokenSource();
            m_oMainTask = MainLoop();
            return true;
        }

        /// <summary>
        /// Stop the bot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            if( m_oMainTask == null) return true;
            m_oCts.Cancel();
            await m_oMainTask;

            if( Quoter != null ) Quoter = null;
            if (Watcher != null)
            {
                await Watcher.Stop();
                Watcher = null;
            }

            return true;
        }
    }
}

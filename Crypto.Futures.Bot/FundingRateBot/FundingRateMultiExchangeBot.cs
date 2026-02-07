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
            Trader = (bPaperTrading ? new CryptoPaperTrader(this) : throw new NotImplementedException());
        }
        public IFundingRateChance[] Chances { get => m_aActiveChances.ToArray(); }

        public IFundingChanceFinder ChanceFinder { get; }
        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public IFuturesExchange[] Exchanges { get=> m_aExchanges; }

        public ICryptoTrader Trader { get ; }

        public CancellationToken CancelToken { get => m_oCts.Token; }


        /// <summary>
        /// Find new funding rate chances
        /// </summary>
        /// <returns></returns>
        private async Task FindNewChances()
        {
            var aChances = await ChanceFinder.FindNewChances();
            if (aChances == null || aChances.Length <= 0) return;
            foreach( var oChance in aChances )
            {
                IFundingRateChance[] aFound = m_aActiveChances.Where(p => p.Currency == oChance.Currency).ToArray();
                if (aFound.Length > 0)
                {
                    Logger.Info($"FundingRateMultiExchangeBot.FindNewChances: Chance for {oChance.Currency} already active");
                    continue;
                }

                bool bRefresh = await oChance.SymbolLong.Refresh(Logger);
                if( !bRefresh || oChance.SymbolLong.OrderbookPrice == null)
                {
                    Logger.Info($"FundingRateMultiExchangeBot.FindNewChances: Cannot refresh long symbol {oChance.SymbolLong.Symbol.ToString()}");
                    continue;
                }
                bRefresh = await oChance.SymbolShort.Refresh(Logger);
                if (!bRefresh || oChance.SymbolShort.OrderbookPrice == null)
                {
                    Logger.Info($"FundingRateMultiExchangeBot.FindNewChances: Cannot refresh short symbol {oChance.SymbolShort.Symbol.ToString()}");
                    continue;
                }

                m_aActiveChances.Add(oChance);
            }
        }


        /// <summary>
        /// Open positions for the given chance
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task OpenPositions( IFundingRateChance oChance )
        {
            decimal nMoney = Setup.MoneyDefinition.Money * Setup.MoneyDefinition.Leverage;
            if (oChance.SymbolLong.PriceOpen == null)
            {
                // B(var oPosition = await Trader.Open(oChance.SymbolLong.Symbol, Setup.MoneyDefinition.Money);
            }
            if (oChance.SymbolShort.PriceOpen == null)
            {

            }

        }


        /// <summary>
        /// Try to close positions for the given chance
        /// </summary>
        /// <param name="oChance"></param>
        /// <returns></returns>
        private async Task TryClosePositions(IFundingRateChance oChance)
        {

        }

        /// <summary>
        /// Main loop   
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            Logger.Info("FundingRateMultiExchangeBot.MainLoop: Starting main loop");    
            while (!m_oCts.IsCancellationRequested)
            {
                try
                {
                    await FindNewChances();
                    foreach( var oChance in m_aActiveChances)
                    {
                        if( !oChance.IsActive ) continue;
                        if ( oChance.SymbolLong.PriceOpen == null || oChance.SymbolShort.PriceOpen == null )
                        {
                            await OpenPositions(oChance);
                            continue;
                        }
                        else
                        {
                            await TryClosePositions(oChance);

                        }
                    }

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
                ExchangeType.BitmartFutures,
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
                if (oFound.Avaliable + oFound.Locked < Setup.MoneyDefinition.Money) continue;
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
            if (m_oMainTask == null) await Stop();
            bool bFound = await FindExchanges();
            if (!bFound)
            {
                Logger.Error("FundingRateMultiExchangeBot.Start: No exchanges found, cannot start bot");
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
            return true;
        }
    }
}

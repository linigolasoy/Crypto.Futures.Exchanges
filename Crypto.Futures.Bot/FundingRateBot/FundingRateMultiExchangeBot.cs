using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.FundingRates;
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
        public FundingRateMultiExchangeBot(IExchangeSetup oSetup, ICommonLogger oLogger, bool bPaperTrading)
        {
            Setup = oSetup;
            Logger = oLogger;
            List<IFuturesExchange> aEchanges = new List<IFuturesExchange>();
            aEchanges.Add(ExchangeFactory.CreateExchange(oSetup, ExchangeType.BingxFutures, oLogger));
            aEchanges.Add(ExchangeFactory.CreateExchange(oSetup, ExchangeType.BitgetFutures, oLogger));

            Exchanges = aEchanges.ToArray();
            ChanceFinder = new FundingChanceFinder(this);
            // Trader = (bPaperTrading ? new PaperTrader(this) : new TraderSocket(this));
        }
        public IFundingRateChance[] Chances => throw new NotImplementedException();

        public IFundingChanceFinder ChanceFinder { get; }
        public IExchangeSetup Setup { get; }

        public ICommonLogger Logger { get; }

        public IFuturesExchange[] Exchanges { get; }

        public ICryptoTrader Trader { get => throw new NotImplementedException(); }

        public CancellationToken CancelToken { get => m_oCts.Token; }


        /// <summary>
        /// Find new funding rate chances
        /// </summary>
        /// <returns></returns>
        private async Task FindNewChances()
        {
            var aChances = await ChanceFinder.FindNewChances();
            if (aChances == null || aChances.Length <= 0) return;
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
        /// Start the bot
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            if (m_oMainTask == null) await Stop();
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

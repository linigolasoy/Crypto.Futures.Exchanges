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

namespace Crypto.Futures.Bot.Trading
{
    /// <summary>
    /// Paper trader implementation for testing purposes    
    /// </summary>
    internal class PaperTrader : ITrader
    {
        private IFuturesExchange[] m_aExchanges;
        private const decimal BALANCE_MULTIPLIER = 3;

        private ConcurrentDictionary<ExchangeType, IBalance> m_aBalances;
        private ConcurrentDictionary<long, ITraderPosition> m_aActivePositions = new ConcurrentDictionary<long, ITraderPosition>();
        private ConcurrentDictionary<long, ITraderPosition> m_aClosedPositions = new ConcurrentDictionary<long, ITraderPosition>();
        private List<IFuturesSymbol> m_aLeverages = new List<IFuturesSymbol>();

        public PaperTrader(ITradingBot oBot)
        {
            Bot = oBot;

            // Create exchanges based on the bot setup  
            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            foreach (var eType in oBot.Setup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oBot.Setup, eType);
                if (!oExchange.Tradeable) continue;
                aExchanges.Add(oExchange);
            }
            m_aExchanges = aExchanges.ToArray();
            // Create initial balances
            m_aBalances = new ConcurrentDictionary<ExchangeType, IBalance>();
            foreach (IFuturesExchange oExchange in m_aExchanges)
            {
                decimal nBalance = Bot.Setup.MoneyDefinition.Money * BALANCE_MULTIPLIER;
                IBalance oBalance = new PaperTraderBalance(oExchange, nBalance);
                m_aBalances.TryAdd(oExchange.ExchangeType, oBalance);
            }
        }
        public ITradingBot Bot { get; }
        public decimal Money { get => Bot.Setup.MoneyDefinition.Money; }
        public decimal Leverage { get => Bot.Setup.MoneyDefinition.Leverage; }

        private const int MAX_DELAY = 800;
        private const int MIN_DELAY = 200;


        public ITraderPosition[] ActivePositions { get => m_aActivePositions.Values.ToArray(); }
        public ITraderPosition[] ClosedPositions { get => m_aClosedPositions.Values.ToArray(); }

        public IBalance[] Balances { get => m_aBalances.Values.ToArray(); }


        /// <summary>
        /// Update trader state, positions, balances, etc.  
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            foreach (ExchangeType eType in m_aBalances.Keys)
            {
                IBalance? oBalance = m_aBalances[eType];
                if (oBalance == null) continue;
                decimal nOpenProfit = (m_aActivePositions.Count <= 0 ? 0 : m_aActivePositions.Values.Where(p => p.Symbol.Exchange.ExchangeType == eType).Sum(p => p.Profit));
                decimal nOpenLocked = (m_aActivePositions.Count <= 0 ? 0 : m_aActivePositions.Values.Where(p => p.Symbol.Exchange.ExchangeType == eType).Sum(p => p.PriceOpen * p.Volume / Bot.Setup.MoneyDefinition.Leverage));
                decimal nClosedProfit = (m_aClosedPositions.Count <= 0 ? 0 : m_aClosedPositions.Values.Where(p => p.Symbol.Exchange.ExchangeType == eType).Sum(p => p.Profit));
                PaperTraderBalance oPaperBalance = (PaperTraderBalance)oBalance;
                oPaperBalance.Balance = oPaperBalance.StartBalance + nOpenProfit + nClosedProfit;
                oPaperBalance.Locked = nOpenLocked; // Locked is the open positions value
            }
            return true;
        }

        /// <summary>
        /// Update balance after position is closed or opened   
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="bClose"></param>
        private void UpdateBalance(ITraderPosition oPosition, bool bClose)
        {
            if (bClose)
            {
                ITraderPosition? oOldPosition = null;
                m_aActivePositions.TryRemove(oPosition.Id, out oOldPosition);
                m_aClosedPositions.TryAdd(oPosition.Id, oPosition);
            }
            else
            {
                m_aActivePositions.TryAdd(oPosition.Id, oPosition);
            }
        }


        /// <summary>
        /// Close a position    
        /// </summary>
        /// <param name="oPosition"></param>
        /// <param name="nPrice"></param>
        /// <returns></returns>
        public async Task<bool> Close(ITraderPosition oPosition, decimal? nPrice = null)
        {
            IWebsocketSymbolData? oData = oPosition.Symbol.Exchange.Market.Websocket.DataManager.GetData(oPosition.Symbol);
            if (oData == null) return false;

            if (oData.LastOrderbookPrice == null) return false;
            int nDelay = Random.Shared.Next(MIN_DELAY, MAX_DELAY);
            await Task.Delay(nDelay);
            if(nPrice == null)
            {
                oPosition.Update();
                ((TraderPosition)oPosition).DateClose = DateTime.Now;
                UpdateBalance(oPosition, true);
                return true;
            }
            int nRetries = 600;
            while (nRetries >= 0)
            {
                await Task.Delay(100);
                decimal nPriceClose = (oPosition.IsLong ? oData.LastOrderbookPrice.BidPrice : oData.LastOrderbookPrice.AskPrice);
                if ((oPosition.IsLong && nPriceClose >= nPrice.Value) || (!oPosition.IsLong && nPriceClose <= nPrice.Value))
                {
                    // oPosition.Update();
                    ((TraderPosition)oPosition).DateClose = DateTime.Now;
                    Bot.Logger.Info($"  Closed position {oPosition.Id} for {oPosition.Symbol.ToString()} at price {oPosition.ActualPrice} with profit {oPosition.Profit} (long: {oPosition.IsLong})");
                    UpdateBalance(oPosition, true);
                    return true;
                }
                nRetries--;
            }

            return false;
        }

        public async Task<ITraderPosition?> Open(IFuturesSymbol oSymbol, bool bLong, decimal nVolume, decimal? nPrice = null)
        {
            IWebsocketSymbolData? oData = oSymbol.Exchange.Market.Websocket.DataManager.GetData(oSymbol);
            if (oData == null) return null;

            if (oData.LastOrderbookPrice == null) return null;
            int nDelay = Random.Shared.Next(MIN_DELAY, MAX_DELAY);
            await Task.Delay(nDelay);
            if (nPrice == null)
            {
                decimal nPriceOpen = (bLong ? oData.LastOrderbookPrice.AskPrice : oData.LastOrderbookPrice.BidPrice);
                ITraderPosition oPosition = new TraderPosition(oSymbol, bLong, nVolume, nPriceOpen);
                UpdateBalance(oPosition, false);
                // Update balance
                return oPosition;
            }
            int nRetries = 600;
            while( nRetries >= 0 )
            {
                await Task.Delay(100);
                decimal nPriceOpen = (bLong ? oData.LastOrderbookPrice.AskPrice : oData.LastOrderbookPrice.BidPrice);
                if( (bLong && nPriceOpen <= nPrice.Value) || (!bLong && nPriceOpen >= nPrice.Value) )
                {
                    ITraderPosition oPosition = new TraderPosition(oSymbol, bLong, nVolume, nPriceOpen);
                    UpdateBalance(oPosition, false);
                    Bot.Logger.Info($"  Opened position {oPosition.Id} for {oSymbol.ToString()} at price {nPriceOpen} with volume {nVolume} (long: {bLong})");
                    return oPosition;
                }
                nRetries--;
            }
            return null; // Failed to open position within retries limit
        }

        /// <summary>
        /// Put leverage
        /// </summary>
        /// <param name="oSymbol"></param>
        /// <returns></returns>
        public async Task<bool> PutLeverage(IFuturesSymbol oSymbol)
        {
            // Simulate leverage change
            if (m_aLeverages.Any(p => p.Exchange.ExchangeType == oSymbol.Exchange.ExchangeType && p.Symbol == oSymbol.Symbol)) return true;
            // Bot.Logger.Info($"  Setting leverage for {oSymbol.ToString()}");
            int nDelay = Random.Shared.Next(MIN_DELAY, MAX_DELAY);
            await Task.Delay(nDelay);
            m_aLeverages.Add(oSymbol);
            return true; // Always success in paper trading
        }
    }
}

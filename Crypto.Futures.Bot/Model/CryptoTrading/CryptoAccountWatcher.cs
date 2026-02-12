using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Model.CryptoTrading.ChangeTrack;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{
    internal class CryptoAccountWatcher : IAccountWatcher
    {

        private ConcurrentDictionary<ExchangeType, IExchangeTrackData> m_aData = new ConcurrentDictionary<ExchangeType, IExchangeTrackData>();
        /// <summary>
        /// Events
        /// </summary>
        /// <param name="aExchanges"></param>
        public CryptoAccountWatcher(IFuturesExchange[] aExchanges)
        {
            Exchanges = aExchanges;
        }
        public IFuturesExchange[] Exchanges { get; }

        public event IAccountWatcher.OnBalanceChangeDelegate? OnBalanceChange;
        public event IAccountWatcher.OnPositionChangeDelegate? OnPositionChange;
        public event IAccountWatcher.OnOrderChangeDelegate? OnOrderChange;


        /// <summary>
        /// Account balances retrieval.
        /// </summary>
        /// <returns></returns>
        public IBalance[] GetBalances()
        {
            List<IBalance> aResult = new List<IBalance>();
            foreach (var oExchange in Exchanges)
            {
                aResult.AddRange(oExchange.Account.WebsocketPrivate.Balances);
            }
            return aResult.ToArray();

        }

        public IOrder[] GetOrders()
        {
            List<IOrder> aResult = new List<IOrder>();
            foreach (var oData in m_aData)
            {
                if( oData.Value.OrderChanged.Count > 0 )
                {
                    aResult.AddRange(oData.Value.OrderChanged.Values.Select(o => o.Item));
                }
            }
            return aResult.ToArray();
        }

        public IPosition[] GetPositions()
        {
            List<IPosition> aResult = new List<IPosition>();
            foreach (var oData in m_aData)
            {
                if (oData.Value.PositionChanged.Count > 0)
                {
                    aResult.AddRange(oData.Value.PositionChanged.Values.Select(o => o.Item));
                }
            }
            return aResult.ToArray();
        }

        public async Task Start()
        {
            foreach (var oExchange in Exchanges)
            {
                // Subscribe to account updates via websocket or polling
                bool bStarted = await oExchange.Account.WebsocketPrivate.Start();
                if( !bStarted)
                {
                    throw new Exception($"Failed to start account watcher for exchange {oExchange.ExchangeType}");
                }

                oExchange.Account.WebsocketPrivate.OnBalance += WatcherOnBalance;
                oExchange.Account.WebsocketPrivate.OnOrder += WatcherOnOrder;
                oExchange.Account.WebsocketPrivate.OnPosition += WatcherOnPosition;
            }
        }

        private void WatcherOnPosition(IPosition oPosition)
        {
            IExchangeTrackData oTrackData = GetTrackData(oPosition.Symbol.Exchange.ExchangeType);

            IChangeTarget<IPosition>? oFound = null;
            if (oTrackData.PositionChanged.TryGetValue(oPosition.Id, out var oChangeTarget))
            {
                oFound = oTrackData.PositionChanged[oPosition.Id];
            }
            bool bEvent = false;
            if ( oFound != null )
            {
                bEvent = oFound.IsChanged();
            }
            else
            {
                bEvent = true;
                oTrackData.PositionChanged[oPosition.Id] = new CryptoPositionChange(oPosition);
            }
            if (bEvent && OnPositionChange != null) OnPositionChange(oPosition);
        }


        private IExchangeTrackData GetTrackData(ExchangeType eType)
        {
            if (!m_aData.ContainsKey(eType))
            {
                m_aData[eType] = new CryptoExchangeTrackData();
            }
            return m_aData[eType];
        }

        /// <summary>
        /// Order change tracker, tracks order changes and triggers events only on actual changes to avoid flooding with unchanged order updates    
        /// </summary>
        /// <param name="oOrder"></param>
        private void WatcherOnOrder(IOrder oOrder)
        {
            IExchangeTrackData oTrackData = GetTrackData(oOrder.Symbol.Exchange.ExchangeType);  

            bool bEvent = !oTrackData.OrderChanged.ContainsKey(oOrder.OrderId);
            if( !bEvent)
            {
                bEvent = oTrackData.OrderChanged[oOrder.OrderId].IsChanged();
            }
            else
            {
                oTrackData.OrderChanged[oOrder.OrderId] = new CryptoOrderChange(oOrder);
            }
            if (bEvent && OnOrderChange != null ) OnOrderChange(oOrder);  
        }

        /// <summary>
        /// Balance change tracker
        /// </summary>
        /// <param name="oBalance"></param>
        private void WatcherOnBalance(IBalance oBalance)
        {
            if( oBalance.Currency != "USDT") return; // Only track USDT balances for now, can be extended to other currencies if needed
            IExchangeTrackData oTrackData = GetTrackData(oBalance.Exchange.ExchangeType);

            bool bEvent = false;
            if ( oTrackData.BalanceChanged != null)
            {
                bEvent = oTrackData.BalanceChanged.IsChanged();
            }
            else
            {
                bEvent = true;
                oTrackData.BalanceChanged = new CryptoBalanceChange(oBalance);
            }
            if( bEvent && OnBalanceChange != null ) OnBalanceChange(oBalance);
            
        }

        public async Task Stop()
        {
            foreach (var oExchange in Exchanges)
            {
                oExchange.Account.WebsocketPrivate.OnBalance -= WatcherOnBalance;
                oExchange.Account.WebsocketPrivate.OnOrder -= WatcherOnOrder;
                oExchange.Account.WebsocketPrivate.OnPosition -= WatcherOnPosition;
                await oExchange.Account.WebsocketPrivate.Stop();
            }
        }
    }
}

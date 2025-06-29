using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Crypto.Futures.Exchanges.WebsocketModel.BaseWebsocketSingle;

namespace Crypto.Futures.Exchanges.WebsocketModel
{

    /// <summary>
    /// Websocket manager for public data   
    /// </summary>
    public class BaseWebsocketManager : IWebsocketPublic
    {
        private List<IWebsocketSingle> m_aChild = new List<IWebsocketSingle>();
        private int m_nActiveChild = -1;
        public BaseWebsocketManager(
            IFuturesMarket oMarket,
            string strUrl,
            IWebsocketParser oParser)
        {
            Market = oMarket;
            Url = strUrl;
            Parser = oParser;
            DataManager = new BaseWsDataManager(oMarket.Exchange);
        }
        public IFuturesMarket Market { get; }

        public BarTimeframe Timeframe { get; set; } = BarTimeframe.M5;

        public IWebsocketParser Parser { get; }

        public IWebsocketDataManager DataManager { get; }

        public string Url { get; }

        public bool Started { get; private set; } = false;
        public delegate Task<bool> StartStopDelegate();

        public StartStopDelegate? StartTask { get; set; } = null;
        public StartStopDelegate? StopTask { get; set; } = null;


        /// <summary>
        /// Sunscriptions for this websocket manager
        /// </summary>
        public IWebsocketSubscription[] Subscriptions 
        {
            get
            {
                List<IWebsocketSubscription> aResult = new List<IWebsocketSubscription>();
                foreach (var oChild in m_aChild)
                {
                    aResult.AddRange(oChild.Subscribed);
                }
                return aResult.ToArray();   
            }
        }

        /// <summary>
        /// Start the websocket manager 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Start()
        {
            if ( m_aChild.Count > 0 )
            {
                await Stop();
            }
            m_aChild.Clear();
            await Task.Delay(2000);
            if (StartTask != null)
            {
                bool bResult = await StartTask();
                if (!bResult) return false;
            }
            await Task.Delay(1000);
            Started = true;
            return true;

        }

        public async Task<bool> Stop()
        {
            if( m_aChild.Count > 0)
            {
                List<Task<bool>> aTasks = new List<Task<bool>>();

                foreach (var oChild in m_aChild)
                {
                    aTasks.Add(oChild.Stop());
                }
                await Task.WhenAll(aTasks);
                m_aChild.Clear();
                await Task.Delay(2000);
            }

            if (StopTask != null)
            {
                bool bResult = await StopTask();
                if (!bResult) return false;
                await Task.Delay(1000);
            }
            return true;
        }

        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            if( m_nActiveChild >= m_aChild.Count ) return null;
            BaseWebsocketSingle? oChild = null;
            if( m_nActiveChild < 0 || m_aChild[m_nActiveChild].Subscribed.Length >= Parser.MaxSubscriptions )
            {
                // Create a new child websocket
                oChild = new BaseWebsocketSingle(this, ++m_nActiveChild);
                bool bStarted = await oChild.Start();
                if (!bStarted)
                {
                    if( Market.Exchange.Logger != null)
                    {
                        Market.Exchange.Logger.Error($"Error starting websocket for {Market.Exchange.ExchangeType.ToString()} ({m_nActiveChild})");
                    }
                    return null;
                }
                if (Market.Exchange.Logger != null)
                {
                    Market.Exchange.Logger.Info($"Created new child {Market.Exchange.ExchangeType.ToString()} ({m_aChild.Count})");
                }
                m_aChild.Add(oChild);
                await Task.Delay(2000);
            }
            else
            {
                oChild = (BaseWebsocketSingle)m_aChild[m_nActiveChild];
            }
            var oResult = await oChild.Subscribe(oSymbol, eSubscriptionType);

            return oResult;
        }
        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol[] aSymbols, WsMessageType eSubscriptionType)
        {
            throw new NotImplementedException();
        }
    }
}

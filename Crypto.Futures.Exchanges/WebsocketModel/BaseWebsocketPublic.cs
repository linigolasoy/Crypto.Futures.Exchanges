using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace Crypto.Futures.Exchanges.WebsocketModel
{

    /// <summary>
    /// Generic exchange websocket implementation
    /// </summary>
    internal class BaseWebsocketPublic
    {

        private List<IFuturesSymbol> m_aSubscribed = new List<IFuturesSymbol>();


        private Task? m_oPingTask = null;
        private CancellationTokenSource m_oCancelToken = new CancellationTokenSource();
        private WebsocketClient? m_oWsClient = null;

        public BaseWebsocketPublic(
            IFuturesMarket oMarket, 
            string strUrl,
            IWebsocketParser oParser) 
        {
            Market = oMarket;   
            Url = strUrl;
            Parser = oParser;
        }

        public IFuturesSymbol[] SubscribedSymbols { get => m_aSubscribed.ToArray(); }

        public IFuturesMarket Market { get; }
        public BarTimeframe Timeframe { get; set; } = BarTimeframe.M1;
        public IWebsocketParser Parser { get; }
        public string Url { get; }

        /// <summary>
        /// Starts websockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            if (m_oWsClient != null)
            {
                await Stop();
            }
            m_oCancelToken = new CancellationTokenSource();

            m_oPingTask = PingTask();
            m_oWsClient = new WebsocketClient(new Uri(Url));
            m_oWsClient.MessageReceived.Subscribe(p => OnMessage(p));
            m_oWsClient.ReconnectionHappened.Subscribe(p => OnReconnection(p));

            await m_oWsClient.Start();
            await Task.Delay(1000);
            return true;
        }

        /// <summary>
        /// Message receiving
        /// </summary>
        /// <param name="oMessage"></param>
        private void OnMessage(ResponseMessage oMessage)
        {
            Console.WriteLine(oMessage);
        }

        /// <summary>
        /// Reconnect
        /// </summary>
        /// <param name="oInfo"></param>
        private void OnReconnection(ReconnectionInfo oInfo)
        {
            if (oInfo.Type == ReconnectionType.Initial) return;

        }


        /// <summary>
        /// Stop websockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            m_oCancelToken.Cancel();
            if (m_oPingTask != null)
            {
                await m_oPingTask;
                m_oPingTask = null;
            }
            if (m_oWsClient != null)
            {
                m_oWsClient.Dispose();
                m_oWsClient = null;
            }
            return true;
        }

        public async Task<bool> Subscribe(IFuturesSymbol oSymbol)
        {
            return await Subscribe( new IFuturesSymbol[] { oSymbol });
        }

        public async Task<bool> Subscribe(IFuturesSymbol[] aSymbols)
        {
            if( m_oWsClient == null ) return false; 
            IFuturesSymbol[] aNew = aSymbols.Where( p=> !m_aSubscribed.Any(q=> p.Symbol == q.Symbol)).ToArray();
            if (aNew.Length <= 0) return true;

            string[] aMessages = Parser.ParseSubscription(aNew, this.Timeframe);

            foreach (string strMessage in aMessages)
            {
                m_oWsClient.Send(strMessage);
                await Task.Delay(200);
            }
            return true;
        }
        public async Task<bool> UnSubscribe(IFuturesSymbol oSymbol)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> UnSubscribe(IFuturesSymbol[]? aSymbols = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ping loop task
        /// </summary>
        /// <returns></returns>
        private async Task PingTask()
        {
            DateTime dLastPing = DateTime.Now;
            while (!m_oCancelToken.IsCancellationRequested)
            {
                DateTime dNow = DateTime.Now;
                if ((dNow - dLastPing).TotalSeconds > Parser.PingSeconds)
                {
                    if (m_oWsClient != null)
                    {
                        m_oWsClient.Send(Parser.ParsePing());
                    }
                    dLastPing = dNow;
                }
                await Task.Delay(2000);
            }

        }

    }
}

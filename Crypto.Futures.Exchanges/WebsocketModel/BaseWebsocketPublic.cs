using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace Crypto.Futures.Exchanges.WebsocketModel
{

    /// <summary>
    /// Generic exchange websocket implementation
    /// </summary>
    public class BaseWebsocketPublic
    {

        private List<IFuturesSymbol> m_aSubscribed = new List<IFuturesSymbol>();

        private Timer? m_oTimer;
        // private Task? m_oPingTask = null;
        private CancellationTokenSource m_oCancelToken = new CancellationTokenSource();
        private WebsocketClient? m_oWsClient = null;

        public delegate Task<bool> StartStopDelegate();

        private bool m_bStarted = false;    
        public StartStopDelegate? StartTask { get; set; } = null;
        public StartStopDelegate? StopTask { get; set; } = null;
        private class TimerData
        {
            public TimerData(WebsocketClient oClient, IWebsocketParser oParser )
            {
                WsClient = oClient;
                Parser = oParser;
            }
            public WebsocketClient WsClient { get; }
            public IWebsocketParser Parser { get; }
        }
        public BaseWebsocketPublic(
            IFuturesMarket oMarket, 
            string strUrl,
            IWebsocketParser oParser) 
        {
            Market = oMarket;   
            Url = strUrl;
            Parser = oParser;
            DataManager = new BaseWsDataManager(oMarket.Exchange);
        }

        public IFuturesSymbol[] SubscribedSymbols { get => m_aSubscribed.ToArray(); }
        public bool Started { get => m_bStarted; }
        public IFuturesMarket Market { get; }
        public BarTimeframe Timeframe { get; set; } = BarTimeframe.M1;
        public IWebsocketParser Parser { get; }

        public IWebsocketDataManager DataManager { get; }    
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
            // m_oPingTask = PingTask();
            m_oWsClient = new WebsocketClient(new Uri(Url));
            m_oWsClient.MessageReceived.Subscribe(p => OnMessage(p));
            m_oWsClient.ReconnectionHappened.Subscribe(p => OnReconnection(p));
            if( Parser.PingSeconds > 0 )
            {
                m_oTimer = new Timer(new TimerCallback( TimerTask), new TimerData(m_oWsClient, Parser), Parser.PingSeconds * 1000, Parser.PingSeconds * 1000);
            }

            await m_oWsClient.Start();
            if( StartTask != null )
            {
                bool bResult = await StartTask();   
                if( !bResult )  return false;
            }
            await Task.Delay(1000);
            m_bStarted = true;
            return true;
        }

        /// <summary>
        /// Timer task for ping
        /// </summary>
        /// <param name="oState"></param>
        private static void TimerTask(object? oState)
        {
            if (oState == null) return;
            if (!(oState is TimerData)) return;
            TimerData oData = (TimerData)oState;
            string strPing = oData.Parser.ParsePing();
            oData.WsClient.Send(strPing);
        }


        private string? GetMessageText( ResponseMessage oMessage)
        {
            if (oMessage.MessageType == WebSocketMessageType.Text) return oMessage.Text;
            if(oMessage.Binary == null) return null;
            var oInput = new MemoryStream(oMessage.Binary);
            if( oInput == null ) return null;
            var oGzip = new GZipStream(oInput, CompressionMode.Decompress);
            if( oGzip == null ) return null;    

            using (var oReader = new StreamReader(oGzip, Encoding.UTF8))
            {
                return oReader.ReadToEnd();
            }
        }
        /// <summary>
        /// Message receiving
        /// </summary>
        /// <param name="oMessage"></param>
        private void OnMessage(ResponseMessage oMessage)
        {
            string? strMessage = null;
            try
            {
                strMessage = GetMessageText(oMessage);
                if (strMessage == null) return ;
                IWebsocketMessage[]? aMessages = Parser.ParseMessage(strMessage);
                if( aMessages == null || aMessages.Length == 0 ) return;
                foreach (var oMsg in aMessages)
                {
                    if( oMsg.MessageType == WsMessageType.Ping && m_oWsClient != null)
                    {
                        m_oWsClient.Send(Parser.ParsePong());
                    }
                    else DataManager.Put(oMsg);
                }

            }
            catch (Exception e)
            {
                if( Market.Exchange.Logger != null )
                {
                    Market.Exchange.Logger.Error("WsOnMessage error", e);
                    if( strMessage != null ) Market.Exchange.Logger.Error(strMessage);
                }
            }
        }

        /// <summary>
        /// Reconnect
        /// </summary>
        /// <param name="oInfo"></param>
        private void OnReconnection(ReconnectionInfo oInfo)
        {
            if (oInfo.Type == ReconnectionType.Initial) return;
            if (Market.Exchange.Logger != null)
            {
                Market.Exchange.Logger.Info($"Reconnect happened on {Market.Exchange.ExchangeType.ToString()}, Resubscribing...");
            }
            try
            {
                Task<bool> oTask = SendSubscriptions(m_aSubscribed.ToArray());
                oTask.Wait();
                if (oTask.Result)
                {
                    if (Market.Exchange.Logger != null)
                    {
                        Market.Exchange.Logger.Info($"Resubscribed on {Market.Exchange.ExchangeType.ToString()}");
                    }
                }

            }
            catch (Exception e)
            {
                if (Market.Exchange.Logger != null)
                {
                    Market.Exchange.Logger.Error($"Error resubscribing {Market.Exchange.ExchangeType.ToString()}", e);
                }
            }



        }


        /// <summary>
        /// Stop websockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            m_oCancelToken.Cancel();
            if ( m_oTimer != null )
            {
                await m_oTimer.DisposeAsync();
                m_oTimer = null;
            }
            if (m_oWsClient != null)
            {
                m_oWsClient.Dispose();
                m_oWsClient = null;
            }
            if (StopTask != null)
            {
                bool bResult = await StopTask();
                if (!bResult) return false;
            }
            m_bStarted = false;

            return true;
        }


        private async Task<bool> SendSubscriptions(IFuturesSymbol[] aSymbols)
        {
            if( m_oWsClient == null ) return false;
            string[] aMessages = Parser.ParseSubscription(aSymbols, this.Timeframe);

            foreach (string strMessage in aMessages)
            {
                m_oWsClient.Send(strMessage);
                await Task.Delay(500);
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
            bool bResult = await SendSubscriptions(aNew);
            if (!bResult) return false;
            m_aSubscribed.AddRange(aNew);
            return true;
        }
        public async Task<bool> UnSubscribe(IFuturesSymbol oSymbol)
        {
            await Task.Delay(500);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unsubscribe to symbols or to all if no argument
        /// </summary>
        /// <param name="aSymbols"></param>
        /// <returns></returns>
        public async Task<bool> UnSubscribe(IFuturesSymbol[]? aSymbols = null)
        {
            if( aSymbols == null )
            {
                aSymbols = SubscribedSymbols;
            }
            foreach( var oSymbol in aSymbols )
            {
                bool bOk = await UnSubscribe( oSymbol );
                if (!bOk)
                {
                    return false;
                }
            }
            return true;
        }


    }
}

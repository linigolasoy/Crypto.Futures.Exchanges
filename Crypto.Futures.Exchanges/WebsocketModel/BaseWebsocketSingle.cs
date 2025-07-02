using Crypto.Futures.Exchanges.Model;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using Websocket.Client;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    internal class BaseWebsocketSingle: IWebsocketSingle
    {
        private List<IWebsocketSubscription> m_aSubscribed = new List<IWebsocketSubscription>();

        private Timer? m_oTimer;
        // private Task? m_oPingTask = null;
        private Task<bool>? m_oResubscribeTask = null;

        public delegate Task<bool> StartStopDelegate();


        // private bool m_bConnected = false; // Used to check if we are connected to the websocket server 

        public BaseWebsocketSingle(
            IWebsocketPublic oManager, 
            int nIndex )
        {
            Manager = oManager;
            Index = nIndex;
        }

        public IWebsocketSubscription[] Subscribed { get => m_aSubscribed.ToArray(); }

        public IWebsocketPublic Manager { get; }
        public int Index { get; }

        private IWebsocketClient? m_oWsClient = null;


        private static ClientWebSocket CreateClient()
        {
            return new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(30),
                    Proxy = null, // Set proxy if needed
                    UseDefaultCredentials = false // Set to true if you want to use default credentials
                }
            };
        }

        private void LogUnknownMessage(string? strMessage)
        {
            if (Manager.Market.Exchange.Logger != null && strMessage != null  && strMessage.ToUpper() != "PONG")
            {
                Manager.Market.Exchange.Logger.Warning($"Unknown message received on {Manager.Market.Exchange.ExchangeType.ToString()} ({Index}): {strMessage}");
            }
        }
        /// <summary>
        /// Message reception
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessageReceived(ResponseMessage oMessage)
        {
            string? strMessage = null;
            if( m_oWsClient == null ) return;   
            try
            {
                strMessage = GetMessageText(oMessage);
                if (strMessage == null) return;


                IWebsocketMessageBase[]? aMessages = Manager.Parser.ParseMessage(strMessage);
                if (aMessages == null || aMessages.Length == 0) { LogUnknownMessage(strMessage); return; }
                foreach (var oMsg in aMessages)
                {
                    if (oMsg.MessageType == WsMessageType.Ping && m_oWsClient != null)
                    {
                        m_oWsClient.Send(Manager.Parser.ParsePong());
                    }
                    else if (oMsg.MessageType == WsMessageType.Subscription)
                    {
                        IWebsocketSubscription oSub = (IWebsocketSubscription)oMsg;
                        if (!m_aSubscribed.Any(p => p.SubscriptionType == oSub.SubscriptionType && p.Symbol.Symbol == oSub.Symbol.Symbol))
                        {
                            m_aSubscribed.Add((IWebsocketSubscription)oMsg);
                        }
                    }
                    else if( oMsg is IWebsocketMessage )
                    {
                        Manager.DataManager.Put((IWebsocketMessage)oMsg);
                    }
                }

            }
            catch (Exception e)
            {
                if (Manager.Market.Exchange.Logger != null)
                {
                    Manager.Market.Exchange.Logger.Error("WsOnMessage error", e);
                    if (strMessage != null) Manager.Market.Exchange.Logger.Error(strMessage);
                }
            }
        }

        private void OnServerConnected(ReconnectionInfo oInfo)
        {
            if(Manager.Market.Exchange.Logger != null)
            {
                Manager.Market.Exchange.Logger.Info($"Reconnected to {Manager.Market.Exchange.ExchangeType.ToString()} ({Index}), {oInfo.Type.ToString()}");
            }

            if( oInfo.Type != ReconnectionType.Initial )
            {
                m_oResubscribeTask = Resubscribe();
            }
            // m_bConnected = true;
        }

        /// <summary>
        /// Starts websockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {

            if( m_oWsClient != null )
            {
                await Stop();   
            }
            m_oWsClient = new WebsocketClient(new Uri(Manager.Url), CreateClient);
            m_oWsClient.LostReconnectTimeout = TimeSpan.FromSeconds(120);
            m_oWsClient.ReconnectTimeout = TimeSpan.FromSeconds(120);
            m_oWsClient.ReconnectionHappened.Subscribe(p=> OnServerConnected(p));
            m_oWsClient.MessageReceived.Subscribe( p=>  OnMessageReceived(p));
            await m_oWsClient.Start();
            if (Manager.Parser.PingSeconds > 0)
            {
                m_oTimer = new Timer(async (o) =>
                {
                    if (m_oWsClient == null || !m_oWsClient.IsRunning) return;
                    string strPing = Manager.Parser.ParsePing();
                    m_oWsClient.Send(strPing);
                }, null, TimeSpan.FromSeconds(Manager.Parser.PingSeconds), TimeSpan.FromSeconds(Manager.Parser.PingSeconds));
            }

            await Task.Delay(2000);
            return true;
        }

        private string? GetMessageText(ResponseMessage oMessage)
        {
            if (oMessage.MessageType == WebSocketMessageType.Text )
            {
                return oMessage.Text;
            }
            if (oMessage.MessageType != WebSocketMessageType.Binary) return null;
            if (oMessage.Binary == null || oMessage.Binary.Length == 0) return null;
            var oInput = new MemoryStream(oMessage.Binary);
            if (oInput == null) return null;
            var oGzip = new GZipStream(oInput, CompressionMode.Decompress);
            if (oGzip == null) return null;

            using (var oReader = new StreamReader(oGzip, Encoding.UTF8))
            {
                return oReader.ReadToEnd();
            }
        }


        /// <summary>
        /// Stop websockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Stop()
        {
            if (m_oTimer != null) ;
            {
                m_oTimer!.Dispose();
                m_oTimer = null;
            }
            if (m_oWsClient != null)
            {
                await m_oWsClient.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "User stopping");
                m_oWsClient.Dispose();
                m_oWsClient = null;
                m_aSubscribed.Clear();  
                await Task.Delay(1000);
            }

            return true;
        }


        private async Task<bool> Resubscribe()
        {
            if (m_oWsClient == null) return false;
            await Task.Delay(2000);

            IWebsocketSubscription[] aExisting = Subscribed;
            m_aSubscribed.Clear();

            foreach (var oSub in aExisting)
            {
                IWebsocketSubscription? oNewSub = await Subscribe(oSub.Symbol, oSub.SubscriptionType);
                if (oNewSub == null)
                {
                    Manager.Market.Exchange.Logger!.Error($"Error resubscribing {oSub.Symbol.Symbol} on {Manager.Market.Exchange.ExchangeType.ToString()} ({Index})");
                }
            }
            return true;

        }

        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol oSymbol, WsMessageType eType)
        {
            if (m_oWsClient == null) return null;
            string? strSubscription = Manager.Parser.ParseSubscription(oSymbol, eType);
            IWebsocketSubscription? oSubscription = null;
            if ( strSubscription == null || string.IsNullOrEmpty(strSubscription))
            {
                oSubscription = new BaseSubscription(eType, oSymbol);
            }
            if (oSubscription == null && strSubscription != null && !string.IsNullOrEmpty(strSubscription))
            {
                /*
                if( Manager.Market.Exchange.Logger != null )
                {
                    Manager.Market.Exchange.Logger.Info($"Sent subscribe {Manager.Market.Exchange.ExchangeType.ToString()} ({strSubscription})");
                }
                */

                bool bSent = m_oWsClient.Send(strSubscription);
                if (!bSent) return null;
                int nRetries = 200;
                while (nRetries > 0)
                {
                    oSubscription = Subscribed.FirstOrDefault(p => p.Symbol.Symbol == oSymbol.Symbol && p.SubscriptionType == eType);
                    if (oSubscription != null)
                    {
                        break;
                    }
                    nRetries--;
                    await Task.Delay(100);
                }
            }
            return oSubscription;
        }

    }
}

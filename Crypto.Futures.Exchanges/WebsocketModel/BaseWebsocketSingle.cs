using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    internal class BaseWebsocketSingle: IWebsocketSingle
    {
        private List<IWebsocketSubscription> m_aSubscribed = new List<IWebsocketSubscription>();

        // private Timer? m_oTimer;
        // private Task? m_oPingTask = null;
        private CancellationTokenSource m_oCancelToken = new CancellationTokenSource();
        private Task? m_oMainTask = null;

        public delegate Task<bool> StartStopDelegate();


        private bool m_bConnected = false; // Used to check if we are connected to the websocket server 

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

        private WatsonWsClient? m_oWsClient = null;
        private async Task MainLoop()
        {
            bool bResubscribe = false;
            while (!m_oCancelToken.IsCancellationRequested)
            {
                DateTime dLastPing = DateTime.Now;
                m_bConnected = false;
                m_oWsClient = new WatsonWsClient(new Uri(Manager.Url));

                m_oWsClient.ServerConnected += OnServerConnected;
                m_oWsClient.MessageReceived += OnMessageReceived;
                m_oWsClient.ServerDisconnected += OnServerDisconnected;
                await m_oWsClient.StartAsync();
                int nRetries = 200;
                while (!m_bConnected && nRetries > 0)
                {
                    await Task.Delay(100);
                    nRetries--;
                }

                if (bResubscribe)
                {
                    await Resubscribe();
                }
                while (m_bConnected && !m_oCancelToken.IsCancellationRequested)
                {
                    await Task.Delay(100);
                    if (Manager.Parser.PingSeconds > 0)
                    {
                        DateTime dNow = DateTime.Now;
                        double nSeconds = (dNow - dLastPing).TotalSeconds;
                        if (nSeconds >= Manager.Parser.PingSeconds)
                        {
                            dLastPing = dNow;
                            string strPing = Manager.Parser.ParsePing();
                            await m_oWsClient.SendAsync(strPing);

                        }
                    }

                }

                if (!m_bConnected)
                {
                    if (Manager.Market.Exchange.Logger != null)
                    {
                        Manager.Market.Exchange.Logger.Warning($"{Manager.Market.Exchange.ExchangeType.ToString()}({Index}) Disconnected. Reconnecting...");
                    }
                }
                await Task.Delay(1000); // Wait before reconnecting 
                await m_oWsClient.StopAsync();
                m_oWsClient.Dispose();
                await Task.Delay(2000);
                m_oWsClient = null;
            }
        }

        private void OnServerDisconnected(object? sender, EventArgs e)
        {
            m_bConnected = false;
        }

        /// <summary>
        /// Message reception
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessageReceived(object? sender, MessageReceivedEventArgs oArgs)
        {
            string? strMessage = null;
            try
            {
                strMessage = GetMessageText(oArgs);
                if (strMessage == null) return;


                IWebsocketMessage[]? aMessages = Manager.Parser.ParseMessage(strMessage);
                if (aMessages == null || aMessages.Length == 0) return;
                foreach (var oMsg in aMessages)
                {
                    if (oMsg.MessageType == WsMessageType.Ping && m_oWsClient != null)
                    {
                        WatsonWsClient oClient = (WatsonWsClient)sender!;
                        oClient.SendAsync(Manager.Parser.ParsePong()).Wait();
                    }
                    else if (oMsg.MessageType == WsMessageType.Subscription)
                    {
                        IWebsocketSubscription oSub = (IWebsocketSubscription)oMsg;
                        if (!m_aSubscribed.Any(p => p.SubscriptionType == oSub.SubscriptionType && p.Symbol.Symbol == oMsg.Symbol.Symbol))
                        {
                            m_aSubscribed.Add((IWebsocketSubscription)oMsg);
                        }
                    }
                    else Manager.DataManager.Put(oMsg);
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

        private void OnServerConnected(object? sender, EventArgs e)
        {
            m_bConnected = true;
        }

        /// <summary>
        /// Starts websockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            if (m_oMainTask != null)
            {
                await Stop();
            }
            m_oCancelToken = new CancellationTokenSource();
            m_oMainTask = MainLoop();   

            await Task.Delay(2000);
            return true;
        }

        private string? GetMessageText(MessageReceivedEventArgs oMessage)
        {
            if (oMessage.MessageType == WebSocketMessageType.Text)
            {
                return Encoding.UTF8.GetString(oMessage.Data);
            }
            if (oMessage.MessageType != WebSocketMessageType.Binary) return null;
            var oInput = new MemoryStream(oMessage.Data.ToArray());
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
            m_oCancelToken.Cancel();

            if (m_oMainTask != null)
            {
                await m_oMainTask;
                m_oMainTask = null;
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
                bool bSent = await m_oWsClient.SendAsync(strSubscription);
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

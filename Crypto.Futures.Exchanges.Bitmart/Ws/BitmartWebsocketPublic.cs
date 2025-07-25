﻿using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Ws
{
    
    /// <summary>
    /// Bitmart websocket
    /// </summary>
    internal class BitmartWebsocketPublic : IWebsocketPublic
    {
        private BitmartMarket m_oMarket;

        private const int MAX_SUBSCRIPTIONS = 180; // Max subscriptions per socket

        private ConcurrentDictionary<int, BitmartSocketSingle> m_aSockets = new ConcurrentDictionary<int, BitmartSocketSingle>();
        private IWebsocketDataManager m_oDataManager;
        public BitmartWebsocketPublic(BitmartMarket oMarket)
        {
            m_oMarket = oMarket;
            m_oDataManager = new BaseWsDataManager(oMarket.Exchange);
        }
        public IFuturesMarket Market { get => m_oMarket; }

        public BarTimeframe Timeframe { get ; set ; } = BarTimeframe.M1;    

        public IWebsocketParser Parser => throw new NotImplementedException();

        public IWebsocketDataManager DataManager { get => m_oDataManager; }

        public string Url => throw new NotImplementedException();

        public bool Started { get; private set; } = false;  

        public IWebsocketSubscription[] Subscriptions 
        {
            get
            {
                List<IWebsocketSubscription> aSubscriptions = new List<IWebsocketSubscription>();
                foreach (var oSocket in m_aSockets.Values)
                {
                    aSubscriptions.AddRange(oSocket.Subscriptions);
                }
                return aSubscriptions.ToArray();
            }
        }

        public async Task<bool> Start()
        {
            Started = true;
            await Task.Delay(200);
            return true;
        }

        public async Task<bool> Stop()
        {
            Started = false;
            await Task.Delay(200);
            return true;
        }

        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            foreach( var oSocket in m_aSockets.Values)
            {
                if (oSocket.Subscriptions.Length >= MAX_SUBSCRIPTIONS)
                {
                    continue; // This socket is full, try next one
                }
                return await oSocket.Subscribe(oSymbol, eSubscriptionType);
            }
            int nNext = m_aSockets.Count;
            if( Market.Exchange.Logger != null)
            {
                Market.Exchange.Logger.Info($"{Market.Exchange.ExchangeType.ToString()} Creating new socket ({nNext})");
            }
            m_aSockets[nNext] = new BitmartSocketSingle(this);
            return await m_aSockets[nNext].Subscribe(oSymbol, eSubscriptionType);
        }

        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol[] aSymbols, WsMessageType eSubscriptionType)
        {
            throw new NotImplementedException();
        }
    }
    
}

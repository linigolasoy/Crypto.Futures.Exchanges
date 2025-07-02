using BingX.Net.Clients;
using BingX.Net.Interfaces.Clients;
using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Bingx.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Ws
{
    internal class BingxSocketSingle
    {
        private BingxWebsocketPublic m_oWebsocket;

        private static Task? m_oFundingLoopTask = null;

        private IBingXSocketClient m_oSocketClient;

        private List<IWebsocketSubscription> m_aSubscriptions = new List<IWebsocketSubscription>();
        public BingxSocketSingle(BingxWebsocketPublic oWebsocket)
        {
            m_oWebsocket = oWebsocket;

            m_oSocketClient = new BingXSocketClient();
        }


        public IWebsocketSubscription[] Subscriptions
        {
            get => m_aSubscriptions.ToArray();
        }


        /// <summary>
        /// Funding rates loop  
        /// </summary>
        /// <returns></returns>
        private async Task FundingRatesLoop()
        {
            while(m_oWebsocket.Started)
            {
                try
                {
                    var oFundingRates = await m_oWebsocket.Market.GetFundingRates(); 
                    if (oFundingRates != null )
                    {
                        foreach(var oFunding in oFundingRates)
                        {
                            if (oFunding == null) continue;
                            m_oWebsocket.DataManager.Put(oFunding);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (m_oWebsocket.Market.Exchange.Logger != null)
                    {
                        m_oWebsocket.Market.Exchange.Logger.Error("Error in Bingx funding rates loop",ex);
                    }
                }
                await Task.Delay(15000); // Wait for 10 seconds before next fetch
            }
        }
        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            IWebsocketSubscription? oResult = null;
            switch (eSubscriptionType)
            {
                case WsMessageType.FundingRate:
                    if(m_oFundingLoopTask == null || m_oFundingLoopTask.IsCompleted)
                    {
                        m_oFundingLoopTask = FundingRatesLoop();
                    }
                    oResult = new BaseSubscription(WsMessageType.FundingRate, oSymbol);
                    break;
                case WsMessageType.OrderbookPrice:
                    var oSubscribeBook = await m_oSocketClient.PerpetualFuturesApi.SubscribeToBookPriceUpdatesAsync(oSymbol.Symbol, OnOrderbook);
                    if (oSubscribeBook == null || !oSubscribeBook.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.OrderbookPrice, oSymbol);
                    break;
                case WsMessageType.LastPrice:
                    var oSubscribeTick = await m_oSocketClient.PerpetualFuturesApi.SubscribeToTickerUpdatesAsync(oSymbol.Symbol, OnTicker);
                    if (oSubscribeTick == null || !oSubscribeTick.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.LastPrice, oSymbol);
                    break;
            }
            if (oResult != null)
            {
                m_aSubscriptions.Add(oResult);
            }
            return oResult;
        }

        private void OnOrderbook(DataEvent<BingXBookTickerUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return;
            IWebsocketMessage oMessage = new BingxOrderbookPrice(oSymbol, oEvent.Data);
            m_oWebsocket.DataManager.Put(oMessage);
        }
        private void OnTicker(DataEvent<BingXFuturesTickerUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return;
            IWebsocketMessage oMessage = new BingxLastPrice(oSymbol, oEvent.Data);
            m_oWebsocket.DataManager.Put(oMessage);
        }
    }
}

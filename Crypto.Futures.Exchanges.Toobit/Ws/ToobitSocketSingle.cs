using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Toobit.Data;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toobit.Net.Clients;
using Toobit.Net.Interfaces.Clients;
using Toobit.Net.Objects.Models;

namespace Crypto.Futures.Exchanges.Toobit.Ws
{
    internal class ToobitSocketSingle
    {
        private ToobitWebsocketPublic m_oWebsocket;

        private IToobitSocketClient m_oSocketClient;
        private static Task? m_oFundingLoopTask = null;


        private List<IWebsocketSubscription> m_aSubscriptions = new List<IWebsocketSubscription>();
        public ToobitSocketSingle(ToobitWebsocketPublic oWebsocket)
        {
            m_oWebsocket = oWebsocket;

            m_oSocketClient = new ToobitSocketClient();
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
            while (m_oWebsocket.Started)
            {
                try
                {
                    var oFundingRates = await m_oWebsocket.Market.GetFundingRates();
                    if (oFundingRates != null)
                    {
                        foreach (var oFunding in oFundingRates)
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
                        m_oWebsocket.Market.Exchange.Logger.Error("Error in Bingx funding rates loop", ex);
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
                    if (m_oFundingLoopTask == null || m_oFundingLoopTask.IsCompleted)
                    {
                        m_oFundingLoopTask = FundingRatesLoop();
                    }
                    oResult = new BaseSubscription(WsMessageType.FundingRate, oSymbol);
                    break;
                case WsMessageType.OrderbookPrice:
                    var oSubscribeBook = await m_oSocketClient.UsdtFuturesApi.SubscribeToPartialOrderBookUpdatesAsync(oSymbol.Symbol, OnOrderBook);
                    if (oSubscribeBook == null || !oSubscribeBook.Success) return null;

                    oResult = new BaseSubscription(WsMessageType.OrderbookPrice, oSymbol);
                    break;
                case WsMessageType.LastPrice:
                    var oSubscribeTicker = await m_oSocketClient.UsdtFuturesApi.SubscribeToTickerUpdatesAsync(oSymbol.Symbol, OnTicker);
                    if (oSubscribeTicker == null || !oSubscribeTicker.Success) return null;

                    oResult = new BaseSubscription(WsMessageType.LastPrice, oSymbol);
                    break;
            }
            if (oResult != null)
            {
                m_aSubscriptions.Add(oResult);
            }
            return oResult;
        }

        private void OnTicker(DataEvent<ToobitTickerUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return;
            IWebsocketMessage oMessageLastPrice = new ToobitLastPrice(oSymbol, oEvent.Data);
            m_oWebsocket.DataManager.Put(oMessageLastPrice);

        }

        private void OnOrderBook(DataEvent<ToobitOrderBookUpdate> oEvent)
        {
            try
            {
                if (oEvent == null || oEvent.Data == null || oEvent.Symbol == null) return;
                IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Symbol);
                if (oSymbol == null) return;

                IWebsocketMessage oMessageOrderbook = new ToobitOrderbookPrice(oSymbol, oEvent.Data);
                m_oWebsocket.DataManager.Put(oMessageOrderbook);
            }
            catch (Exception ex) 
            {
                return;
            }

        }
    }
}

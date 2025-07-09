using BitMart.Net.Clients;
using BitMart.Net.Interfaces.Clients;
using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Bitmart.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Ws
{
    
    internal class BitmartSocketSingle
    {
        private BitmartWebsocketPublic m_oWebsocket;

        private IBitMartSocketClient m_oSocketClient;

        private List<IWebsocketSubscription> m_aSubscriptions = new List<IWebsocketSubscription>(); 
        public BitmartSocketSingle(BitmartWebsocketPublic oWebsocket)
        {
            m_oWebsocket = oWebsocket;

            m_oSocketClient = new BitMartSocketClient();
        }


        public IWebsocketSubscription[] Subscriptions
        {
            get => m_aSubscriptions.ToArray();
        }

        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            IWebsocketSubscription? oResult = null;
            switch ( eSubscriptionType )
            {
                case WsMessageType.FundingRate:
                    var oSubscribeFunding = await m_oSocketClient.UsdFuturesApi.SubscribeToFundingRateUpdatesAsync(oSymbol.Symbol, OnFundingRate);
                    if (oSubscribeFunding == null || !oSubscribeFunding.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.FundingRate, oSymbol);
                    break;
                case WsMessageType.OrderbookPrice:
                    var oSubscribeBook = await m_oSocketClient.UsdFuturesApi.SubscribeToOrderBookSnapshotUpdatesAsync(oSymbol.Symbol, 5, OnOrderbook);
                    if (oSubscribeBook == null || !oSubscribeBook.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.OrderbookPrice, oSymbol);
                    break;
                case WsMessageType.LastPrice:
                    var oSubscribeTick = await m_oSocketClient.UsdFuturesApi.SubscribeToTickerUpdatesAsync(oSymbol.Symbol, OnTicker);
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

        private void OnFundingRate( DataEvent<BitMartFundingRateUpdate> oEvent )
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return;
            IWebsocketMessage oMessage = new BitmartFundingRate(oSymbol, oEvent.Data);
            m_oWebsocket.DataManager.Put(oMessage);
        }
        private void OnOrderbook(DataEvent<BitMartFuturesFullOrderBookUpdate> oEvent)
        {
            try
            {
                if (oEvent == null || oEvent.Data == null) return;
                IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
                if (oSymbol == null) return;
                if (oEvent.Data.Asks == null || oEvent.Data.Bids == null) return;
                if ( oEvent.Data.Asks.Length <= 0 || oEvent.Data.Bids.Length <= 0) return;   
                IWebsocketMessage oMessage = new BitmartOrderbookPrice(oSymbol, oEvent.Data);
                m_oWebsocket.DataManager.Put(oMessage);

            }
            catch (Exception ex)
            {
                if (m_oWebsocket.Market.Exchange.Logger != null)
                {
                    m_oWebsocket.Market.Exchange.Logger.Error("BitmartSocketSingle.OnOrderbook", ex);
                }
            }
        }
        private void OnTicker(DataEvent<BitMartFuturesTickerUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return;
            IWebsocketMessage oMessage = new BitmartLastPrice(oSymbol, oEvent.Data, oEvent.ReceiveTime);
            m_oWebsocket.DataManager.Put(oMessage);
        }
    }
    
}

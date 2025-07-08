using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Interfaces.Clients;
using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Ws
{
    internal class BitgetSocketSingle
    {
        private BitgetWebsocketPublic m_oWebsocket;

        private IBitgetSocketClient m_oSocketClient;

        private List<IWebsocketSubscription> m_aSubscriptions = new List<IWebsocketSubscription>();
        public BitgetSocketSingle(BitgetWebsocketPublic oWebsocket)
        {
            m_oWebsocket = oWebsocket;

            m_oSocketClient = new BitgetSocketClient();
        }


        public IWebsocketSubscription[] Subscriptions
        {
            get => m_aSubscriptions.ToArray();
        }

        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            IWebsocketSubscription? oResult = null;
            switch (eSubscriptionType)
            {
                case WsMessageType.FundingRate:
                    var oSubscribeFund = await m_oSocketClient.FuturesApiV2.SubscribeToTickerUpdatesAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, OnTicker);
                    if (oSubscribeFund == null || !oSubscribeFund.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.FundingRate, oSymbol);
                    break;
                case WsMessageType.OrderbookPrice:
                    var oSubscribeBook = await m_oSocketClient.FuturesApiV2.SubscribeToOrderBookUpdatesAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, 5, OnOrderBook);
                    //.SubscribeToTickerUpdatesAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, OnTicker);
                    if (oSubscribeBook == null || !oSubscribeBook.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.OrderbookPrice, oSymbol);
                    break;
                case WsMessageType.LastPrice:
                    oResult = new BaseSubscription(WsMessageType.LastPrice, oSymbol);
                    break;
            }
            if (oResult != null)
            {
                m_aSubscriptions.Add(oResult);
            }
            return oResult;
        }

        private void OnTicker(DataEvent<BitgetFuturesTickerUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return;
            IWebsocketMessage oMessageFunding = new BitgetFundingRate(oSymbol, oEvent.Data);
            m_oWebsocket.DataManager.Put(oMessageFunding);
            IWebsocketMessage oMessageLastPrice = new BitgetLastPrice(oSymbol, oEvent.Data);
            m_oWebsocket.DataManager.Put(oMessageLastPrice);

        }
        private void OnOrderBook(DataEvent<BitgetOrderBookUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Symbol == null) return;
            IFuturesSymbol? oSymbol = m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Symbol);
            if (oSymbol == null) return;
            IWebsocketMessage oMessageOrderbook = new BitgetOrderbookPrice(oSymbol, oEvent.Data);
            m_oWebsocket.DataManager.Put(oMessageOrderbook);
            // IWebsocketMessage oMessageFunding = new BitgetFundingRate(oSymbol, oEvent.Data);
            // m_oWebsocket.DataManager.Put(oMessageFunding);
            // IWebsocketMessage oMessageLastPrice = new BitgetLastPrice(oSymbol, oEvent.Data);
            // m_oWebsocket.DataManager.Put(oMessageLastPrice);

        }
    }
}

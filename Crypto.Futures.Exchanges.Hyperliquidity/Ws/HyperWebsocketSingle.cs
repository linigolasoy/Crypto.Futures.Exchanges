using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Ws
{

    internal class HyperLastPrice : ILastPrice
    {

        public HyperLastPrice(IFuturesSymbol oSymbol, DateTime dDate, decimal nPrice) 
        {
            Symbol = oSymbol;
            DateTime = dDate;
            Price = nPrice;
        }
        public decimal Price { get; private set; }

        public DateTime DateTime { get; private set; }

        public IFuturesSymbol Symbol { get; }

        public WsMessageType MessageType { get => WsMessageType.LastPrice; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if( oMessage.MessageType != WsMessageType.LastPrice) return;
            ILastPrice oPrice = (ILastPrice)oMessage;
            Price = oPrice.Price;
            DateTime = oPrice.DateTime;
        }
    }
    internal class HyperWebsocketSingle
    {

        private HyperWebsocketPublic m_oWebsocket;
        private IHyperLiquidSocketClient m_oSocketClient;
        private bool m_bSubscribedToPrice = false;

        private List<IWebsocketSubscription> m_aSubscriptions = new List<IWebsocketSubscription>();
        public HyperWebsocketSingle( HyperWebsocketPublic oPublic ) 
        {
            m_oWebsocket = oPublic;
            m_oSocketClient = new HyperLiquidSocketClient();
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
                    // var oSubscribeFund = await m_oSocketClient.FuturesApi.SubscribeToBookTickerUpdatesAsync(oSymbol.Symbol, OnTicker);
                    // if (oSubscribeFund == null || !oSubscribeFund.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.FundingRate, oSymbol);
                    break;
                case WsMessageType.OrderbookPrice:
                    var oSubscribeBook = await m_oSocketClient.FuturesApi.SubscribeToBookTickerUpdatesAsync(oSymbol.Symbol, OnTicker);
                    //.SubscribeToTickerUpdatesAsync(BitgetProductTypeV2.UsdtFutures, oSymbol.Symbol, OnTicker);
                    if (oSubscribeBook == null || !oSubscribeBook.Success) return null;
                    oResult = new BaseSubscription(WsMessageType.OrderbookPrice, oSymbol);
                    break;
                case WsMessageType.LastPrice:
                    if( !m_bSubscribedToPrice )
                    {
                        var oSubscribePrice = await m_oSocketClient.FuturesApi.SubscribeToPriceUpdatesAsync(OnPriceUpdate);
                        if (oSubscribePrice == null || !oSubscribePrice.Success) return null;
                        m_bSubscribedToPrice = true;
                    }
                    oResult = new BaseSubscription(WsMessageType.LastPrice, oSymbol);
                    break;
            }
            if (oResult != null)
            {
                m_aSubscriptions.Add(oResult);
            }
            return oResult;
        }

        private void OnTicker(DataEvent<HyperLiquidBookTicker> oEvent)
        {
            throw new NotImplementedException();
        }
        private void OnPriceUpdate(DataEvent<Dictionary<string,decimal>> oEvent)
        {
            if( oEvent.Data == null) return;
            foreach (var kvp in oEvent.Data)
            {
                string strSymbol = kvp.Key;
                decimal nPrice = kvp.Value;
                if (strSymbol.Contains('/')) continue;

                IFuturesSymbol? oSymbol = this.m_oWebsocket.Market.Exchange.SymbolManager.GetSymbol(strSymbol);
                if (oSymbol == null) continue;
                ILastPrice oPrice = new HyperLastPrice(oSymbol, DateTime.Now, nPrice);
                m_oWebsocket.DataManager.Put(oPrice);
            }
        }
    }
}

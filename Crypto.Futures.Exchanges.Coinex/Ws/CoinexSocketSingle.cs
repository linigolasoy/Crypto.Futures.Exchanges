
using CoinEx.Net.Clients;
using CoinEx.Net.Interfaces.Clients;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Objects.Sockets;

namespace Crypto.Futures.Exchanges.Coinex.Ws
{


    internal class CoinexSocketSingle
    {

        private List<IWebsocketSubscription> m_aSubscriptions = new List<IWebsocketSubscription>();

        private ICoinExSocketClient m_oSocketClient;
        public CoinexSocketSingle(CoinexWebsocketPublic websocketPublic)
        {
            Websocket = websocketPublic;
            m_oSocketClient = new CoinExSocketClient();
        }
        public IWebsocketPublic Websocket { get; }

        public IWebsocketSubscription[] Subscriptions
        {
            get
            {
                return m_aSubscriptions.ToArray();
            }
        }

        public async Task<IWebsocketSubscription?> Subscribe(IFuturesSymbol oSymbol, WsMessageType eSubscriptionType)
        {
            switch (eSubscriptionType)
            {
                case WsMessageType.LastPrice:
                    return new BaseSubscription(eSubscriptionType, oSymbol);
                case WsMessageType.OrderbookPrice:
                    var oSubBook = await m_oSocketClient.FuturesApi.SubscribeToOrderBookUpdatesAsync(oSymbol.Symbol, 5, null, true, OnOrderbook);
                    //.SubscribeToBookPriceUpdatesAsync(oSymbol.Symbol, OnBookPrice);
                    if (oSubBook == null || !oSubBook.Success) return null;
                    return new BaseSubscription(eSubscriptionType, oSymbol);
                // Add more cases for other message types as needed
                case WsMessageType.FundingRate:
                    var oSubTick = await m_oSocketClient.FuturesApi.SubscribeToTickerUpdatesAsync(new string[] { oSymbol.Symbol }, OnTicker);
                    if (oSubTick == null || !oSubTick.Success) return null;
                    return new BaseSubscription(eSubscriptionType, oSymbol);
            }
            return null;
        }


        private void OnTicker(DataEvent<CoinExFuturesTickerUpdate[]> oEvent)
        {
            if( oEvent == null ) return;
            if( oEvent.Data == null || oEvent.Data.Length <= 0) return;
            foreach( var oData in oEvent.Data)
            {
                IFuturesSymbol? oSymbol = Websocket.Market.Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) return; // Symbol not found, skip this update
                ILastPrice oLast = new CoinexLastPrice(oSymbol, oData, oEvent.DataTime);
                Websocket.DataManager.Put(oLast);
                IFundingRate oFunding = new CoinexFundingRate(oSymbol, oData);
                Websocket.DataManager.Put(oFunding);
            }
            return;
        }
        private void OnBookPrice(DataEvent<CoinExBookPriceUpdate> oEvent)
        {
            if(oEvent == null) return;
            if (oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = Websocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return; // Symbol not found, skip this update
            IOrderbookPrice oOrderbookPrice = new CoinexOrderbookPrice(oSymbol, oEvent.Data, oEvent.DataTime);
            Websocket.DataManager.Put(oOrderbookPrice);
            return;
        }

        private void OnOrderbook(DataEvent<CoinExOrderBook> oEvent)
        {
            if (oEvent == null) return;
            if (oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = Websocket.Market.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null) return; // Symbol not found, skip this update
            IOrderbookPrice oOrderbookPrice = new CoinexOrderbookPrice(oSymbol, oEvent.Data);
            Websocket.DataManager.Put(oOrderbookPrice);
            return;
        }

    }
}
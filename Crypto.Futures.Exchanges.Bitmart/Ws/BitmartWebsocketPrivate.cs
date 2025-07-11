using BitMart.Net.Clients;
using BitMart.Net.Interfaces.Clients;
using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Bitmart.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Ws
{
    internal class BitmartWebsocketPrivate : BasePrivateManager, IWebsocketPrivate
    {
        private IBitMartSocketClient m_oSocketClient;
        public BitmartWebsocketPrivate(IFuturesAccount oAccount) :
            base(oAccount)
        {
            m_oSocketClient = new BitMartSocketClient();
            m_oSocketClient.SetApiCredentials(new ApiCredentials(Account.Exchange.ApiKey.ApiKey, Account.Exchange.ApiKey.ApiSecret, Account.Exchange.ApiKey.ApiPassword));
        }


        /// <summary>
        /// Start sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {

            IBalance[]? aStartBalances = await Account.Exchange.Account.GetBalances();
            if( aStartBalances == null ) return false;
            foreach (var oBalance in aStartBalances) Put(oBalance);

            var oResultBalance = await m_oSocketClient.UsdFuturesApi.SubscribeToBalanceUpdatesAsync(PrivateOnBalance);
            if (oResultBalance == null || !oResultBalance.Success) return false;

            var oResultOrder = await m_oSocketClient.UsdFuturesApi.SubscribeToOrderUpdatesAsync(PrivateOnOrder);
            if (oResultOrder == null || !oResultOrder.Success) return false;

            var oResultPosition = await m_oSocketClient.UsdFuturesApi.SubscribeToPositionUpdatesAsync(PrivateOnPosition);
            if (oResultPosition == null || !oResultPosition.Success) return false;

            return true;
        }

        public async Task<bool> Stop()
        {
            await Task.Delay(1000);
            return true;
        }

        private void PrivateOnBalance(DataEvent<BitMartFuturesBalanceUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IBalance oBalance = new BitmartBalance(Account.Exchange, oEvent.Data);
            Put(oBalance);

        }


        private void PrivateOnOrder(DataEvent<BitMartFuturesOrderUpdateEvent[]> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            foreach (var oData in oEvent.Data)
            {
                IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oData.Order.Symbol);
                if (oSymbol == null) continue;
                IOrder oOrder = new BitmarOrderMine(oSymbol, oData);
                Put(oOrder);
            }
        }

        private void PrivateOnPosition(DataEvent<BitMartPositionUpdate[]> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            List<IPosition> aUpdated = new List<IPosition>();
            foreach (var oData in oEvent.Data)
            {
                IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue;
                IPosition oPosition = new BitmartPositionMine(oSymbol, oData);
                aUpdated.Add(oPosition);
                Put(oPosition);
            }

            IPosition[] aToClose = Positions.Where(p => p.IsOpen && !aUpdated.Any(q => p.Id == q.Id)).ToArray();
            foreach (var oToClose in aToClose)
            {
                BitmartPositionMine oMine = (BitmartPositionMine)oToClose;
                oMine.IsOpen = false;
                Put(oMine);
            }


        }
    }
}

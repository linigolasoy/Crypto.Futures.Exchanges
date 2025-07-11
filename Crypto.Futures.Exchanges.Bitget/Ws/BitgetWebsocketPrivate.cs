using Bitget.Net.Clients;
using Bitget.Net.Enums;
using Bitget.Net.Interfaces.Clients;
using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Bitget.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Ws
{
    internal class BitgetWebsocketPrivate : BasePrivateManager, IWebsocketPrivate
    {

        private IBitgetSocketClient m_oSocketClient;

        public BitgetWebsocketPrivate( IFuturesAccount oAccount) :
            base(oAccount) 
        { 
            m_oSocketClient = new BitgetSocketClient();
            m_oSocketClient.SetApiCredentials(new ApiCredentials(Account.Exchange.ApiKey.ApiKey, Account.Exchange.ApiKey.ApiSecret, Account.Exchange.ApiKey.ApiPassword));
        }


        /// <summary>
        /// Start sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            var oResultBalance = await m_oSocketClient.FuturesApiV2.SubscribeToBalanceUpdatesAsync(BitgetProductTypeV2.UsdtFutures, PrivateOnBalance);
            if( oResultBalance == null || !oResultBalance.Success) return false;

            var oResultOrder = await m_oSocketClient.FuturesApiV2.SubscribeToOrderUpdatesAsync(BitgetProductTypeV2.UsdtFutures, PrivateOnOrder);
            if( oResultOrder == null || ! oResultOrder.Success) return false;

            var oResultPosition = await m_oSocketClient.FuturesApiV2.SubscribeToPositionUpdatesAsync(BitgetProductTypeV2.UsdtFutures, PrivateOnPosition);
            if (oResultPosition == null || !oResultPosition.Success) return false;

            return true;    
        }

        public async Task<bool> Stop()
        {
            await Task.Delay(1000);
            return true;
        }

        private void PrivateOnBalance(DataEvent<BitgetFuturesBalanceUpdate[]> oEvent)
        {
            if( oEvent == null || oEvent.Data == null ) return;
            foreach (var oData in oEvent.Data)
            {
                IBalance oBalance = new BitgetBalanceMine(Account.Exchange, oData);
                Put(oBalance);
            }

        }
        private void PrivateOnOrder(DataEvent<BitgetFuturesOrderUpdate[]> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            foreach (var oData in oEvent.Data)
            {
                IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if( oSymbol == null ) continue;
                IOrder oOrder = new BitgetOrderMine(oSymbol, oData);
                Put(oOrder);
            }
        }

        private void PrivateOnPosition(DataEvent<BitgetPositionUpdate[]> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            List<IPosition> aUpdated = new List<IPosition>();   
            foreach (var oData in oEvent.Data)
            {
                IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oData.Symbol);
                if (oSymbol == null) continue;
                IPosition oPosition = new BitgetPositionMine(oSymbol, oData);
                aUpdated.Add(oPosition);
                Put(oPosition);
            }

            IPosition[] aToClose = Positions.Where(p=> p.IsOpen && !aUpdated.Any(q=> p.Id == q.Id)).ToArray(); 
            foreach( var oToClose in aToClose)
            {
                BitgetPositionMine oMine = (BitgetPositionMine)oToClose;
                oMine.IsOpen = false;
                Put(oMine);
            }


        }

    }
}

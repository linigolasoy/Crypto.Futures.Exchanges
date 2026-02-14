using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.Toobit.Data;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
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
    internal class ToobitWebsocketPrivate : BasePrivateManager, IWebsocketPrivate
    {

        private IToobitSocketClient m_oSocketClient;
        private IToobitRestClient m_oRestClient;

        private string? m_strListenKey = null;
        private Timer? m_oTimer = null;


        public ToobitWebsocketPrivate(IFuturesAccount oAccount) :
            base(oAccount) 
        {

            var oCredentials = new ApiCredentials(Account.Exchange.ApiKey.ApiKey, Account.Exchange.ApiKey.ApiSecret);
            m_oSocketClient = new ToobitSocketClient();
            m_oSocketClient.SetApiCredentials(oCredentials);

            m_oRestClient = new ToobitRestClient();
            m_oRestClient.SetApiCredentials(oCredentials);

        }






        public async Task<bool> Start()
        {
            IBalance[]? aStartBalances = await Account.Exchange.Account.GetBalances();
            if (aStartBalances == null) return false;
            foreach (var oBalance in aStartBalances) Put(oBalance);


            // IPosition[]? aPositions = await Account.Exchange.Account.GetPositions();
            // if (aPositions == null) return false;

            var oStream = await m_oRestClient.UsdtFuturesApi.Account.StartUserStreamAsync();
            if (oStream == null || !oStream.Success || oStream.Data == null) return false;
            m_strListenKey = oStream.Data;

            m_oTimer = new Timer(OnTimer, null, 55 * 60 * 1000, 55 * 60 * 1000);

            var oSubscribe = await m_oSocketClient.UsdtFuturesApi.SubscribeToUserDataUpdatesAsync(
                m_strListenKey,
                OnAccount,
                OnCaptureOrder,
                OnCapturePosition,
                OnCaptureTrade);

            if( oSubscribe == null) return false;
            if( !oSubscribe.Success) return false;
            return true;
        }

        private void OnCaptureTrade(DataEvent<ToobitUserTradeUpdate[]> oEvent)
        {
            return;
        }

        private void OnCapturePosition(DataEvent<ToobitPositionUpdate[]> oEvent)
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Data.Length <= 0) return;
            string? strSymbol = oEvent.Symbol;
            IFuturesSymbol? oSymbolEvent = null;
            if (strSymbol != null) oSymbolEvent = Account.Exchange.SymbolManager.GetSymbol(strSymbol);
            foreach (var item in oEvent.Data)
            {
                string strItemSymbol = item.Symbol;
                IFuturesSymbol? oFound = Account.Exchange.SymbolManager.GetSymbol(strItemSymbol);
                if (oFound == null) oFound = oSymbolEvent;
                if (oFound == null) continue;
                bool bClosed = (item.PositionQuantity <= 0);
                if (bClosed)
                {
                    if (bClosed == this.Positions.Length <= 0) continue;
                    bClosed = true;
                }
                IPosition oPosition = new ToobitPositionMine(oFound, item);
                Put(oPosition);
            }

            return;
        }

        private void OnAccount(DataEvent<ToobitAccountUpdate> oEvent)
        {
            return;
        }

        private void OnCaptureOrder(DataEvent<ToobitFuturesOrderUpdate[]> oEvent)
        {
            if (oEvent == null || oEvent.Data == null || oEvent.Data.Length <= 0 ) return;  
            string? strSymbol = oEvent.Symbol;
            IFuturesSymbol? oSymbolEvent = null;    
            if( strSymbol != null ) oSymbolEvent = Account.Exchange.SymbolManager.GetSymbol(strSymbol);
            foreach (var item in oEvent.Data)
            {
                string strItemSymbol = item.Symbol;
                IFuturesSymbol? oFound = Account.Exchange.SymbolManager.GetSymbol(strItemSymbol);
                if (oFound == null) oFound = oSymbolEvent;
                if (oFound == null) continue;
                IOrder oOrder = new ToobitOrderMine(oFound, item);    
                Put(oOrder);
            }

        }

        private void OnTimer(object? oState)
        {
            if (m_strListenKey == null) return;
            if (Account.Exchange.Logger != null)
            {
                Account.Exchange.Logger.Info("Toobit Trying to extend WS key...");
            }
            var oTask = m_oRestClient.UsdtFuturesApi.Account.KeepAliveUserStreamAsync(m_strListenKey);
            oTask.Wait();
            if (Account.Exchange.Logger != null)
            {
                if (oTask.Result == null || !oTask.Result.Success)
                {
                    Account.Exchange.Logger.Error("Toobit Could not extend WS key!!!!!!!!!!!!!!!!");
                }
                else Account.Exchange.Logger.Info("Toobit Extended WS key...");
            }
            return;
        }

        public async Task<bool> Stop()
        {
            if( m_strListenKey == null) { return false; }
            await m_oSocketClient.UsdtFuturesApi.UnsubscribeAllAsync();
            await Task.Delay(1000);


            await m_oRestClient.UsdtFuturesApi.Account.StopUserStreamAsync(m_strListenKey);

            if( m_oTimer != null ) { m_oTimer.Dispose(); m_oTimer = null; }
            return true;    
        }
    }
}

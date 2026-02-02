using BingX.Net.Clients;
using BingX.Net.Interfaces.Clients;
using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Bingx.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bingx.Ws
{
    internal class BingxWebsocketPrivate : BasePrivateManager, IWebsocketPrivate
    {
        private IBingXSocketClient m_oSocketClient;
        private IBingXRestClient m_oRestClient; 
        private string? m_strSocketKey = null;

        private Timer? m_oTimer = null;
        public BingxWebsocketPrivate(IFuturesAccount oAccount) :
            base(oAccount)
        {
            m_oSocketClient = new BingXSocketClient();
            m_oSocketClient.SetApiCredentials(new ApiCredentials(Account.Exchange.ApiKey.ApiKey, Account.Exchange.ApiKey.ApiSecret, Account.Exchange.ApiKey.ApiPassword));
            m_oRestClient = new BingXRestClient();
            m_oRestClient.SetApiCredentials(new ApiCredentials(Account.Exchange.ApiKey.ApiKey, Account.Exchange.ApiKey.ApiSecret));
        }


        /// <summary>
        /// Start sockets
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {

            IBalance[]? aStartBalances = await Account.Exchange.Account.GetBalances();
            if (aStartBalances == null) return false;
            foreach (var oBalance in aStartBalances) Put(oBalance);

            var oStartStream = await m_oRestClient.PerpetualFuturesApi.Account.StartUserStreamAsync();
            if( oStartStream == null || !oStartStream.Success ) return false;
            m_strSocketKey = oStartStream.Data;

            m_oTimer = new Timer(OnTimer, null, 55*60*1000, 55*60*1000);

            var oResult = await m_oSocketClient.PerpetualFuturesApi.SubscribeToUserDataUpdatesAsync(
                    m_strSocketKey,
                    OnAccountUpdates,
                    OnOrderUpdates,
                    null,
                    OnExpired                 
                );


            // var oResultBalance = await m_oSocketClient.SubscribeToBalanceUpdatesAsync(PrivateOnBalance);
            // if (oResultBalance == null || !oResultBalance.Success) return false;

            // var oResultOrder = await m_oSocketClient.UsdFuturesApi.SubscribeToOrderUpdatesAsync(PrivateOnOrder);
            // if (oResultOrder == null || !oResultOrder.Success) return false;

            // if (oResultPosition == null || !oResultPosition.Success) return false;

            return true;
        }

        private void OnTimer(object? oState)
        {
            if (m_strSocketKey == null ) return;
            if( Account.Exchange.Logger != null )
            {
                Account.Exchange.Logger.Info("Bingx Trying to extend WS key...");
            }
            var oTask = m_oRestClient.PerpetualFuturesApi.Account.KeepAliveUserStreamAsync(m_strSocketKey);
            oTask.Wait();
            if (Account.Exchange.Logger != null)
            {
                if( oTask.Result == null || !oTask.Result.Success )
                {
                    Account.Exchange.Logger.Error("Bingx Could not extend WS key!!!!!!!!!!!!!!!!");
                }
                else Account.Exchange.Logger.Info("Bingx Extended WS key...");
            }
            return;
        }

        public async Task<bool> Stop()
        {
            if( m_strSocketKey != null )
            {
                var oEndStream = await m_oRestClient.PerpetualFuturesApi.Account.StopUserStreamAsync(m_strSocketKey);
                m_strSocketKey = null;  
            }
            if( m_oTimer != null )
            {
                m_oTimer.Dispose();
                m_oTimer = null;
            }
            await Task.Delay(1000);

            return true;
        }

        private void OnAccountUpdates(DataEvent<BingXFuturesAccountUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            if( oEvent.Data == null ) return;   
            if( oEvent.Data.Update.Balances != null && oEvent.Data.Update.Balances.Length > 0 )
            {
                foreach( var oData in oEvent.Data.Update.Balances )
                {
                    IBalance oBalance = new BingxBalance(Account.Exchange, oData);
                    Put(oBalance);
                }
            }
            if (oEvent.Data.Update.Positions != null && oEvent.Data.Update.Positions.Length > 0)
            {
                foreach (var oData in oEvent.Data.Update.Positions)
                {
                    IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oData.Symbol);
                    if(oSymbol==null) continue;
                    IPosition oPosition = new BingxPosition(oSymbol, oData);
                    Put(oPosition);
                }
            }

            return;
            // IBalance oBalance = new BitmartBalance(Account.Exchange, oEvent.Data);
            // Put(oBalance);

        }


        private void OnOrderUpdates(DataEvent<BingXFuturesOrderUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oEvent.Data.Symbol);
            if (oSymbol == null)
            {
                if (oEvent.Symbol != null)
                {
                    oSymbol = Account.Exchange.SymbolManager.GetSymbol(oEvent.Symbol);
                }
                if (oSymbol == null) return;
            }
            IOrder oOrder = new BingxOrderMine(oSymbol, oEvent.Data);
            Put(oOrder);
            return;
        }

        private void OnExpired(DataEvent<BingXListenKeyExpiredUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
        }
    }
}

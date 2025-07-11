using CoinEx.Net.Clients;
using CoinEx.Net.Interfaces.Clients;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Coinex.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Ws
{
    internal class CoinexWebsocketPrivate : BasePrivateManager, IWebsocketPrivate
    {

        private ICoinExSocketClient m_oSocketClient;
        public CoinexWebsocketPrivate( IFuturesAccount oAccount ) :
            base( oAccount )
        { 
            m_oSocketClient = new CoinExSocketClient();
            m_oSocketClient.SetApiCredentials(new ApiCredentials(Account.Exchange.ApiKey.ApiKey, Account.Exchange.ApiKey.ApiSecret));
        }

        public async Task<bool> Start()
        {
            IBalance[]? aBalances = await Account.GetBalances();
            if( aBalances == null ) return false;
            foreach( var oBalance in aBalances ) Put(oBalance);

            var oResultBalance = await m_oSocketClient.FuturesApi.SubscribeToBalanceUpdatesAsync(OnPrivateBalance);
            if (oResultBalance == null || !oResultBalance.Success)  return false;

            var oResultPos = await m_oSocketClient.FuturesApi.SubscribeToPositionUpdatesAsync(OnPrivatePosition);
            if (oResultPos == null || !oResultPos.Success) return false;

            var oResultOrder = await m_oSocketClient.FuturesApi.SubscribeToOrderUpdatesAsync(OnPrivateOrder);
            if (oResultOrder == null || !oResultOrder.Success) return false;

            return true;
        }

        public async Task<bool> Stop()
        {
            await Task.Delay(1000); 
            return true;
        }

        private void OnPrivateBalance(DataEvent<CoinExFuturesBalance[]> oEvent)
        {
            if( oEvent == null || oEvent.Data == null ) return;
            foreach( var oItem in oEvent.Data )
            {
                IBalance oBalance = new CoinexBalance(Account.Exchange, oItem );
                Put(oBalance);
            }
        }
        private void OnPrivatePosition(DataEvent<CoinExPositionUpdate> oEvent)
        {
            if( oEvent ==null || oEvent.Data == null ) return; 
            IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oEvent.Data.Position.Symbol);
            if (oSymbol == null) return;
            IPosition oPosition = new CoinexPosition(oSymbol, oEvent.Data);
            Put(oPosition);
            return;
        }

        private void OnPrivateOrder(DataEvent<CoinExFuturesOrderUpdate> oEvent)
        {
            if (oEvent == null || oEvent.Data == null) return;
            IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oEvent.Data.Order.Symbol);
            if( oSymbol == null ) return;
            IOrder oOrder = new CoinexOrderMine(oSymbol, oEvent.Data);
            Put(oOrder);
        }


    }
}

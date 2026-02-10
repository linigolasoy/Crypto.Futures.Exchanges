using Crypto.Futures.Exchanges.Hyperliquidity.Data;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects.Sockets;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Hyperliquidity.Ws
{
    internal class HyperWebsocketPrivate : BasePrivateManager, IWebsocketPrivate
    {

        private IHyperLiquidSocketClient m_oSocketClient;

        private ConcurrentDictionary<long, HyperLiquidUserTrade> m_aTrades = new ConcurrentDictionary<long, HyperLiquidUserTrade>();
        public HyperWebsocketPrivate(IFuturesAccount oAccount) : base(oAccount)
        {
            m_oSocketClient = new HyperLiquidSocketClient();
            m_oSocketClient.SetApiCredentials(new ApiCredentials(oAccount.Exchange.ApiKey.ApiKey, oAccount.Exchange.ApiKey.ApiSecret));
        }



        public async Task<bool> Start()
        {
            var oAccountUpdates = await m_oSocketClient.FuturesApi.SubscribeToUserUpdatesAsync(Account.Exchange.ApiKey.ApiKey, OnUserUpdates);
            if (oAccountUpdates == null || !oAccountUpdates.Success) return false;

            var oOrderUpdates = await m_oSocketClient.FuturesApi.SubscribeToOrderUpdatesAsync(Account.Exchange.ApiKey.ApiKey, OnOrderUpdates);
            if (oOrderUpdates == null || !oOrderUpdates.Success) return false;

            var pTradeUpdates = await m_oSocketClient.FuturesApi.SubscribeToUserTradeUpdatesAsync(Account.Exchange.ApiKey.ApiKey, OnTradeUpdates);
            return true;
        }

        private void OnTradeUpdates(DataEvent<HyperLiquidUserTrade[]> oEvent)
        {
            if (oEvent.Data == null || oEvent.Data.Length == 0) return;
            foreach( var oData in oEvent.Data)
            {
                if (oData == null ) continue;
                if( !m_aTrades.ContainsKey(oData.TradeId))
                {
                    m_aTrades.TryAdd(oData.TradeId, oData);
                    IPosition[] aPositions = Positions;
                    if( aPositions.Length > 0 )
                    {
                        foreach (var oPos in aPositions)
                        {
                            if (oPos.Symbol.Symbol == oData.Symbol && oPos.IsOpen)
                            {
                                if( oData.Direction == Direction.CloseLong && oPos.IsLong ||
                                    oData.Direction == Direction.CloseShort && !oPos.IsLong )
                                {
                                    HyperPosition oHyperPos = (HyperPosition)oPos;
                                    oHyperPos.Close(oData.Price);
                                    Put(oPos);
                                }
                                return;
                            }
                        }

                    }
                }
            }

            return;
        }

        private List<OrderStatus> m_aStatuses = new List<OrderStatus>();
        private void OnOrderUpdates(DataEvent<HyperLiquidOrderStatus[]> oEvent)
        {
            if(oEvent.Data == null || oEvent.Data.Length == 0) return;
            foreach( var oData in oEvent.Data )
            {
                if (oData == null || oData.Order.ExchangeSymbol == null) return;
                IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oData.Order.ExchangeSymbol);
                if (oSymbol == null) continue;
                IOrder oNewOrder = new HyperOrder(oSymbol, oData);
                Put(oNewOrder);
            }
        }

        private void PutOrder(HyperLiquidOrder? oOrder)
        {
            if( oOrder == null || oOrder.ExchangeSymbol == null ) return;
            IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oOrder.ExchangeSymbol);
            if (oSymbol == null) return;
            IOrder oNewOrder = new HyperOrder(oSymbol, oOrder);
            Put(oNewOrder);
        }

        private void PutPosition(HyperLiquidPosition? oPosition)
        {
            if (oPosition == null) return;
            IFuturesSymbol? oSymbol = Account.Exchange.SymbolManager.GetSymbol(oPosition.Position.Symbol);
            if (oSymbol == null) return;
            IPosition oPos = new HyperPosition(oSymbol, oPosition);
            Put(oPos);
        }

        private void PutBalance(HyperLiquidMarginSummary oSummary)
        {
            IBalance oBalance = new HyperBalance(Account.Exchange, oSummary);
            Put(oBalance);
        }

        private void OnUserUpdates(DataEvent<HyperLiquidUserUpdate> oEvent)
        {
            if (oEvent.Data == null) return;
            try
            {

                // Open orders process
                if( oEvent.Data.OpenOrders != null && oEvent.Data.OpenOrders.Length > 0)
                {
                    foreach (var oOrder in oEvent.Data.OpenOrders)
                    {
                        PutOrder(oOrder);
                    }
                }
                // position process
                if ( oEvent.Data.FuturesInfo.Positions != null && oEvent.Data.FuturesInfo.Positions.Length > 0)
                {
                    foreach (var oPosition in oEvent.Data.FuturesInfo.Positions)
                    {
                        PutPosition(oPosition);
                    }
                }
                // If we have position opened, close
                // TODO: Update balances
                PutBalance(oEvent.Data.FuturesInfo.CrossMarginSummary);
            }
            catch (Exception ex)
            {
                if (Account.Exchange.Logger != null) Account.Exchange.Logger?.Error($"HyperWebsocketPrivate.OnUserUpdates: {ex.Message}");
            }

            return;
        }


        public async Task<bool> Stop()
        {
            await Task.Delay(1000); return true;    
        }
    }
}

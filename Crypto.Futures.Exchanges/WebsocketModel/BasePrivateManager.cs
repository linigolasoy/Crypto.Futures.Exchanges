using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Base private manage
    /// </summary>
    public class BasePrivateManager : IPrivateWebsocketManager
    {

        private ConcurrentDictionary<string, IBalance> m_aBalances = new ConcurrentDictionary<string, IBalance>();
        private ConcurrentDictionary<string, IOrder> m_aOrders = new ConcurrentDictionary<string, IOrder>();
        private ConcurrentDictionary<string, IPosition> m_aPositions = new ConcurrentDictionary<string, IPosition>();
        private ConcurrentDictionary<int, IPosition> m_aPositionClosed = new ConcurrentDictionary<int, IPosition>();

        private static int m_nLastId = 0;
        public BasePrivateManager( IFuturesAccount oAccount ) 
        { 
            Account = oAccount;
        }
        public IFuturesAccount Account { get; }

        public IBalance[] Balances => throw new NotImplementedException();

        public IOrder[] Orders => throw new NotImplementedException();

        public IPosition[] Positions { get => m_aPositions.Values.ToArray(); }

        public event IPrivateWebsocketManager.OnOrderDelegate? OnOrder = null;
        public event IPrivateWebsocketManager.OnPositionDelegate? OnPosition = null;
        public event IPrivateWebsocketManager.OnBalanceDelegate? OnBalance = null;

        private void PutOrder( IOrder oOrder )
        {
            m_aOrders.AddOrUpdate(oOrder.OrderId, oOrder, (p,q) => { q.Update(oOrder); return q; } );
            if (OnOrder != null)
            {
                if (m_aOrders.TryGetValue(oOrder.OrderId, out IOrder? oFound))
                {
                    OnOrder(oFound);
                }
            }
        }

        private void PutBalance( IBalance oBalance )
        {
            m_aBalances.AddOrUpdate(oBalance.Currency, oBalance, (p,q) => { q.Update(oBalance); return q; }  );
            if (OnBalance != null)
            {
                if (m_aBalances.TryGetValue(oBalance.Currency, out IBalance? oFound))
                {
                    OnBalance(oFound);
                }
            }
        }

        private void PutPosition( IPosition oPosition )
        {
            m_aPositions.AddOrUpdate(oPosition.Id, oPosition, (p, q) => { q.Update(oPosition); return q; });
            if (OnPosition != null)
            {
                if (m_aPositions.TryGetValue(oPosition.Id, out IPosition? oFound))
                {
                    OnPosition(oFound);
                    if( !oFound.IsOpen )
                    {
                        int nId = m_nLastId++;
                        m_aPositionClosed.TryAdd(nId, oFound);
                        m_aPositions.TryRemove(oFound.Id, out oFound);  
                    }
                }
            }

        }
        public void Put(IWebsocketMessageBase oMessage)
        {
            if (oMessage is IBalance)
            {
                PutBalance((IBalance)oMessage);
            }
            else if (oMessage is IPosition)
            {
                PutPosition((IPosition)oMessage);
            }
            else if (oMessage is IOrder)
            {
                PutOrder((IOrder)oMessage);
            }
        }
    }
}

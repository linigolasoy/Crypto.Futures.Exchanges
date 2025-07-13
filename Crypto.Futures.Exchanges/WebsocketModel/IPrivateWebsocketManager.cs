using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.WebsocketModel
{
    /// <summary>
    /// Websocket manager
    /// </summary>
    public interface IPrivateWebsocketManager
    {
        public delegate void OnOrderDelegate(IOrder oOrder);
        public delegate void OnPositionDelegate(IPosition oPosition);
        public delegate void OnBalanceDelegate(IBalance oBalance);


        public event OnOrderDelegate? OnOrder;
        public event OnPositionDelegate? OnPosition;
        public event OnBalanceDelegate? OnBalance;
        public IFuturesAccount Account { get; }

        public IBalance[] Balances { get; }
        public IOrder[] Orders { get; }

        public IOrder? GetOrder(string strOrderId);
        public IPosition[] Positions { get; }

        public void Put(IWebsocketMessageBase oMessage); 
    }
}

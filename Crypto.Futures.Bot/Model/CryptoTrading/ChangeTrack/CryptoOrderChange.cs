using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading.ChangeTrack
{
    internal class CryptoOrderChange : IChangeTarget<IOrder>
    {

        private ModelOrderStatus m_eStatus;
        public CryptoOrderChange(IOrder oOrder) 
        {
            Item = oOrder;
            m_eStatus = oOrder.Status;
            LastChange = DateTime.Now;
        }

        public DateTime LastChange { get; private set; }

        public IOrder Item { get; }

        public bool IsChanged()
        {
            if( Item.Status != m_eStatus)
            {
                m_eStatus = Item.Status;
                LastChange = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}

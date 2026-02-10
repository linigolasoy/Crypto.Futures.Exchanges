using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading.ChangeTrack
{
    internal class CryptoPositionChange : IChangeTarget<IPosition>
    {
        private bool m_bOpen = true;
        private decimal m_nQuantity = 0;
        public CryptoPositionChange(IPosition oPosition)
        {
            Item = oPosition;
            m_bOpen = oPosition.IsOpen;
            m_nQuantity = oPosition.Quantity;
            LastChange = DateTime.Now;
        }

        public DateTime LastChange { get; private set; }

        public IPosition Item { get; }

        public bool IsChanged()
        {
            if (m_bOpen != Item.IsOpen ||m_nQuantity != Item.Quantity)
            {
                m_bOpen = Item.IsOpen;
                m_nQuantity = Item.Quantity;
                LastChange = DateTime.Now;
                return true;
            }
            return false;   
        }
    }
}

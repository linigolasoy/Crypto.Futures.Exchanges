using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading.ChangeTrack
{
    internal class CryptoBalanceChange : IChangeTarget<IBalance>
    {
        private decimal m_nTotal = 0;

        public CryptoBalanceChange(IBalance oBalance)
        {
            Item = oBalance;
            LastChange = DateTime.Now;
            m_nTotal = oBalance.Balance + oBalance.Avaliable + oBalance.Locked;
        }   
        public DateTime LastChange { get; private set; }

        public IBalance Item { get; }

        public bool IsChanged()
        {
            decimal nTotal = Item.Balance + Item.Avaliable + Item.Locked;
            if (nTotal != m_nTotal)
            {
                m_nTotal = nTotal;
                LastChange = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}

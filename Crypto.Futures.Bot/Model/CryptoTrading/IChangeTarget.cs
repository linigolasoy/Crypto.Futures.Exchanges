using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{
    /// <summary>
    /// Interface for change target, used to track changes in balances, positions and orders
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IChangeTarget<T>
    {
        public DateTime LastChange { get; } 
        public bool IsChanged();
        public T Item { get; }
    }
}

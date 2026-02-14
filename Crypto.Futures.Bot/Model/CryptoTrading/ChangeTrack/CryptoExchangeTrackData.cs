using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading.ChangeTrack
{
    /// <summary>
    /// Crypto exchange track data implementation
    /// </summary>
    internal class CryptoExchangeTrackData : IExchangeTrackData
    {

        private ConcurrentDictionary<string, IChangeTarget<IOrder>> m_aOrders = new ConcurrentDictionary<string, IChangeTarget<IOrder>>();
        private ConcurrentDictionary<string, IChangeTarget<IPosition>> m_aPositions = new ConcurrentDictionary<string, IChangeTarget<IPosition>>();
        private ConcurrentDictionary<string, IChangeTarget<IPosition>> m_aPositionHistory = new ConcurrentDictionary<string, IChangeTarget<IPosition>>();
        public CryptoExchangeTrackData() 
        { 
        }
        public IChangeTarget<IBalance>? BalanceChanged { get; set; }= null;

        public ConcurrentDictionary<string, IChangeTarget<IOrder>> OrderChanged { get => m_aOrders; }

        public ConcurrentDictionary<string, IChangeTarget<IPosition>> PositionChanged { get => m_aPositions; }
        public ConcurrentDictionary<string, IChangeTarget<IPosition>> PositionHistoryChanged { get => m_aPositionHistory; }
    }
}

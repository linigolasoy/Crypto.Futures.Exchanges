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
    internal interface IExchangeTrackData
    {
        public IChangeTarget<IBalance>? BalanceChanged { get; set; }
        public ConcurrentDictionary<string, IChangeTarget<IOrder>> OrderChanged { get; }
        public ConcurrentDictionary<string, IChangeTarget<IPosition>> PositionChanged { get; }
        public ConcurrentDictionary<string, IChangeTarget<IPosition>> PositionHistoryChanged { get; }
    }
}

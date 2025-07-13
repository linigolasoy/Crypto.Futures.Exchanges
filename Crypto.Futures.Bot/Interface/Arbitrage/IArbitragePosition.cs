using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.Arbitrage
{

    /// <summary>
    /// Possible status
    /// </summary>
    public enum ArbitragePositionStatus
    {
        Check,
        Canceled,
        TryOpen,
        Open,
        TryClose,
        Closed
    }

    /// <summary>
    /// Positions
    /// </summary>
    public interface IArbitragePosition
    {
        public ICryptoBot Bot { get; }
        public int Id { get; }  
        public ArbitragePositionStatus Status { get; }
        public IArbitrageChance Chance { get; }

        public decimal Amount { get; }

        public decimal Percent { get; }
        public decimal PercentProfit { get; }
        public decimal Profit { get; }

        public ICryptoPosition? PositionLong { get; }
        public ICryptoPosition? PositionShort { get; }

        public Task Runner { get; }    
    }
}

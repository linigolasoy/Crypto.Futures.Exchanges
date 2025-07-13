using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.Arbitrage
{
    /// <summary>
    /// Chance manager, handles chances
    /// </summary>
    public interface IArbitrageChanceManager
    {
        public ICryptoBot Bot { get; }

        public IArbitragePosition[] ActivePositions { get; }
        public IArbitragePosition[] ClosedPositions { get; }

        public Task<bool> Add(IArbitrageChance[] aChances);
    }
}

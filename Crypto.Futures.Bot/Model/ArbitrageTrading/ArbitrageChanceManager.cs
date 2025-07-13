using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.Arbitrage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{
    /// <summary>
    /// Chance manager
    /// </summary>
    internal class ArbitrageChanceManager : IArbitrageChanceManager
    {

        private ConcurrentDictionary<int, IArbitragePosition> m_aActivePositions = new ConcurrentDictionary<int, IArbitragePosition>();
        private ConcurrentDictionary<int, IArbitragePosition> m_aClosedPositions = new ConcurrentDictionary<int, IArbitragePosition>();

        private Task? m_oMainTask = null;
        // private int m_nExecuted = 0;
        public ArbitrageChanceManager(ICryptoBot oBot) 
        { 
            Bot = oBot;
        }
        public ICryptoBot Bot { get; }

        public IArbitragePosition[] ActivePositions { get => m_aActivePositions.Values.ToArray(); }

        public IArbitragePosition[] ClosedPositions { get => m_aClosedPositions.Values.ToArray(); }

        public async Task<bool> Add(IArbitrageChance[] aChances)
        {
            foreach( var oChance in aChances )
            {
                // if (m_nExecuted > 1) continue;
                if (ActivePositions.Length >= Bot.Setup.Arbitrage.MaxOperations) return true;
                if (ActivePositions.Any(p => p.Chance.Currency == oChance.Currency)) continue;
                if (m_oMainTask == null) m_oMainTask = MainLoop();
                // m_nExecuted++;
                IArbitragePosition oPosition = new ArbitragePosition(Bot, oChance);
                m_aActivePositions.TryAdd(oPosition.Id, oPosition);
            }
            await Task.Delay(100);
            return true;
        }

        /// <summary>
        /// Main loop
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {

            while( !Bot.CancelToken.IsCancellationRequested)
            {
                IArbitragePosition[] aPending = ActivePositions;
                foreach (var oPosition in aPending)
                {
                    if( !oPosition.Runner.IsCompleted) continue;
                    if( m_aActivePositions.TryRemove(oPosition.Id, out IArbitragePosition? oValue) )
                    {
                        Bot.Logger.Info($"Completed chance {oValue.Chance.ToString()} with status {oValue.Status.ToString()} and profit {oValue.Profit}");
                        m_aClosedPositions.TryAdd(oValue.Id, oValue);
                    }
                }
                await Task.Delay(100);
            }
        }
    }
}

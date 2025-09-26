using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.TelegramSignals.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.TelegramSignals.BackTest
{
    /// <summary>
    /// Back test chance implementation
    /// </summary>
    internal class BackTestChance : ISignalBackTesterChance
    {
        /// <summary>
        /// Back test chance constructor    
        /// </summary>
        /// <param name="oSignal"></param>
        /// <param name="nMoney"></param>
        /// <param name="oSymbol"></param>
        public BackTestChance(ISignal oSignal, decimal nMoney, IFuturesSymbol oSymbol )
        {
            Signal = oSignal;
            // Calculate entry price
            EntryPrice = (oSignal.IsLong ? oSignal.Entries.Max() : oSignal.Entries.Min());
            // Calculate quantity
            decimal nDiffSl = Math.Abs(EntryPrice - oSignal.StopLoss);
            int nDecimals = oSymbol.QuantityDecimals;
            int nContractDecimals = -(int)Math.Log10((double)oSymbol.ContractSize);
            decimal nQtySl = nMoney / nDiffSl;

            nDecimals += nContractDecimals; // Adjust for contract size decimals
            if (nDecimals < 0)
            {
                decimal nPow = (decimal)Math.Pow(10, -nDecimals); // Calculate the power of 10 for rounding
                nQtySl = Math.Floor(nQtySl / nPow) * nPow; // Round to the nearest contract size
            }
            else
            {
                nQtySl = Math.Round(nQtySl, nDecimals); // Round to the correct number of decimals
            }
            Quantity = nQtySl;
        }
        public ISignal Signal { get; }

        public DateTime? DateOpen { get; internal set; } = null;

        public DateTime? DateClose { get; internal set; } = null;

        public decimal EntryPrice { get; }

        public decimal ExitPrice { get; internal set; } = 0;

        public decimal Profit { get; internal set; } = 0;
        public decimal Pnl { get; internal set; } = 0;

        public decimal Quantity { get; } = 0;
    }
}

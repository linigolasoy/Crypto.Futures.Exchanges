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
    /// Back test result
    /// </summary>
    internal class BackTestResult : ISignalBackTesterResult
    {


        public  BackTestResult(ISignalBackTester oTester, DateTime dFrom, DateTime dTo )
        {
            BackTester = oTester;
            StartDate = dFrom;
            EndDate = dTo;
        }

        private List<ISignalBackTesterChance> m_aChances = new List<ISignalBackTesterChance>();
        private List<ISignalBackTesterSnapshot> m_aSnapshots = new List<ISignalBackTesterSnapshot>();
        public ISignalBackTester BackTester { get; }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

        public int Count { get; internal set; } = 0;

        public int Won { get; internal set; } = 0;

        public decimal Profit { get; internal set; } = 0;

        public ISignalBackTesterChance[] Chances { get => m_aChances.ToArray(); }

        public ISignalBackTesterSnapshot[] Snapshots { get => m_aSnapshots.ToArray(); }

        /// <summary>
        /// Create a new signal chance based on the signal provided 
        /// </summary>
        /// <param name="oSignal"></param>
        /// <returns></returns>
        internal ISignalBackTesterChance NewSignal( ISignal oSignal, IFuturesSymbol oSymbol, decimal nMoney )
        {
            BackTestChance oChance = new BackTestChance(oSignal, nMoney, oSymbol);
            m_aChances.Add(oChance); // add the chance to the list
            return oChance;
        }

        private void DoOpen( BackTestChance oChance, IBar oBar )
        {
            // decimal nTakeProfit = oChance.Signal.TakeProfit[0];
            // decimal nStopLoss = oChance.Signal.StopLoss;
            if ( oChance.Signal.CloseDate != null )
            {
                if(oBar.DateTime >= oChance.Signal.CloseDate.Value) // If the bar date is after the close date, do not open the position
                {
                    oChance.DateOpen = oBar.DateTime;
                    oChance.DateClose = oBar.DateTime;
                    return;
                }
            }

            decimal nEntryMin = oChance.Signal.Entries.Min(); // Get the minimum entry price
            decimal nEntryMax = oChance.Signal.Entries.Max(); // Get the maximum entry price
            if ( oBar.Low <= nEntryMax && oBar.High >= nEntryMin) // If the bar low is less than or equal to the maximum entry price and the bar high is greater than or equal to the minimum entry price, open the position
                // ( oChance.Signal.IsLong && oBar.Low <= oChance.EntryPrice ) ||
                // ( !oChance.Signal.IsLong && oBar.High >= oChance.EntryPrice )
            // )
            {
                oChance.DateOpen = oBar.DateTime;
                this.Count++; // Increment the count of chances
            }


            return;
        }

        private void DoCheck(BackTestChance oChance, IBar oBar)
        {
            decimal nTakeProfit = oChance.Signal.TakeProfit[0];
            decimal nStopLoss = oChance.Signal.StopLoss;
            oChance.ExitPrice = oBar.Close; // Update the exit price with the current bar close price
            if ( oChance.Signal.IsLong )
            {
                if( oBar.Low <= nStopLoss )
                {
                    oChance.ExitPrice = nStopLoss; // Set the exit price to the stop loss price
                    oChance.DateClose = oBar.DateTime; // Set the close date to the current bar date
                }
                else if( oBar.High >= nTakeProfit )
                {
                    oChance.ExitPrice = nTakeProfit; // Set the exit price to the take profit price
                    oChance.DateClose = oBar.DateTime; // Set the close date to the current bar date
                    this.Won++; // Increment the won count if the take profit is hit
                }
            }
            else
            {
                if (oBar.High >= nStopLoss)
                {
                    oChance.ExitPrice = nStopLoss; // Set the exit price to the stop loss price
                    oChance.DateClose = oBar.DateTime; // Set the close date to the current bar date
                }
                else if (oBar.Low <= nTakeProfit)
                {
                    oChance.ExitPrice = nTakeProfit; // Set the exit price to the take profit price
                    oChance.DateClose = oBar.DateTime; // Set the close date to the current bar date
                    this.Won++; // Increment the won count if the take profit is hit
                }

            }
            decimal nMultipler = oChance.Signal.IsLong ? 1 : -1; // Set the multiplier based on the signal direction  
            decimal nProfit = (oChance.ExitPrice - oChance.EntryPrice) * oChance.Quantity * nMultipler; // Calculate the profit based on the exit price, entry price, quantity and direction
            if(oChance.DateClose != null)
            {
                oChance.Profit = nProfit; // Set the profit to the calculated profit
                oChance.Pnl = 0; // Set the unrealized profit to the calculated profit
                this.Profit += nProfit; // Increment the won count if the take profit is hit
            }
            else
            {
                oChance.Pnl = nProfit;
            }
            return;

        }

        private void UpdateSnap(BackTestChance oChance, IBar oBar)
        {
            return;

        }

        internal bool Update( ISignalBackTesterChance oChance, IBar oBar )
        {
            // Update the chance with the bar data
            BackTestChance oBack = (BackTestChance)oChance; 

            if(oBack.DateOpen == null) DoOpen(oBack, oBar); // If the open date is not set, open the position   
            else DoCheck(oBack, oBar); // Otherwise check the position  

            UpdateSnap(oBack, oBar); // Update the snapshot with the bar data
            return (oBack.DateClose == null); // Return false if the position is closed  
        }
    }
}

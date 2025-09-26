using Crypto.Futures.Bot;
using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.Arbitrage;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Crypto.Futures.TelegramSignals;
using Crypto.Futures.TelegramSignals.Signals;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Tests
{
    [TestClass]
    public sealed class TelegramTests
    {
        private static string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";


        [TestMethod]
        public async Task TelegramHistoryTest()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            ICommonLogger oLogger = ExchangeFactory.CreateLogger(oSetup, "TelegramTest");
            Assert.IsNotNull(oLogger, "Logger should not be null.");

            ISignalScanner oScanner = TelegramFactory.CreateSignalScanner(oSetup, oLogger, SignalType.EveningTraderMidCap);
            Assert.IsNotNull(oScanner, "Signal scanner should not be null.");

            IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, ExchangeType.MexcFutures, oLogger);
            Assert.IsNotNull(oExchange, "Exchange should not be null.");

            // DateTime dFrom = DateTime.Today.AddMonths(-1); // Get history from one month ago    
            DateTime dFrom = DateTime.Today.AddMonths(-1); // Get history from one month ago    
            DateTime dTo = DateTime.Today.AddDays(-1);

            decimal nMoney = 50;
            int nOperations = 10;
            ISignalBackTester oBackTester = TelegramFactory.CreateBackTester(oExchange, oScanner, nMoney, nOperations);
            Assert.IsNotNull(oBackTester, "Backtester should not be null.");

            var oResult = await oBackTester.Run(dFrom, dTo, nMoney, nOperations);
            Assert.IsNotNull(oResult, "Backtester result should not be null.");
            /*

            ISignal[]? aSignals = await oScanner.GetHistory(dFrom);
            Assert.IsNotNull(aSignals, "Signals should not be null.");
            Assert.IsTrue(aSignals.Length > 0, "There should be at least one signal in the history.");


            IFuturesSymbol[] aSymbols = oExchange.SymbolManager.GetAllValues();
            decimal nMoney = 5;
            decimal nTotalProfit = 0;
            int nConsecutiveLosses = 0; // Track consecutive losses 
            int nMaxConsecutiveLosses = 0; // 
            int nTotalWon = 0;
            int nTotal = 0;

            foreach ( var oSignal in aSignals.OrderBy(p=> p.SignalDate))
            {
                IFuturesSymbol? oSymbol = aSymbols.FirstOrDefault(p => p.Base == oSignal.Currency && p.Quote == "USDT");
                if (oSymbol == null) continue;
                DateTime dFromSig = oSignal.SignalDate;
                DateTime dToSig = dFromSig.AddDays(7);
                IBar[]? aBars = await oExchange.History.GetBars(oSymbol, BarTimeframe.M15, dFromSig, dToSig);
                if (aBars == null || aBars.Length <= 0 ) continue;
                nTotal++;

                decimal nEntryMin = oSignal.Entries.Min();
                decimal nEntryMax = oSignal.Entries.Max();

                decimal nEntry = (oSignal.IsLong) ? nEntryMax : nEntryMin;
                decimal nDiffSl = Math.Abs(nEntry - oSignal.StopLoss);  
                int nDecimals = oSymbol.QuantityDecimals;   
                int nContractDecimals = -(int)Math.Log10((double)oSymbol.ContractSize);
                decimal nQtySl = nMoney / nDiffSl;

                nDecimals += nContractDecimals; // Adjust for contract size decimals
                if(nDecimals < 0) 
                {
                    decimal nPow = (decimal)Math.Pow(10, -nDecimals); // Calculate the power of 10 for rounding
                    nQtySl = Math.Floor(nQtySl / nPow) * nPow; // Round to the nearest contract size
                }
                else
                {
                    nQtySl = Math.Round(nQtySl, nDecimals); // Round to the correct number of decimals
                }
                decimal nMoneyQty = nQtySl * nEntry;
                int nLeverage = (int)Math.Ceiling(nMoneyQty / nMoney);
                bool bEnter = false;
                decimal nProfit = 0;
                foreach ( var oBar in aBars.OrderBy(p=> p.DateTime))
                {
                    if( !bEnter )
                    {
                        if( oSignal.CloseDate != null )
                        {
                            if (oBar.DateTime >= oSignal.CloseDate.Value) break;
                        }
                        // if (oSignal.IsLong && oBar.High >= oSignal.TakeProfit[0]) break;
                        // if (!oSignal.IsLong && oBar.Low <= oSignal.TakeProfit[0]) break;
                        if ( oBar.High <= nEntryMax || oBar.Low >= nEntryMin )
                        {
                            bEnter = true;
                        }
                        continue;
                    }

                    if(oSignal.IsLong)
                    {
                        if (oBar.Low <= oSignal.StopLoss)
                        {
                            nProfit = oSignal.StopLoss - nEntryMax;
                            break;
                        }
                        if (oBar.High >= oSignal.TakeProfit[0])
                        {
                            nProfit = oSignal.TakeProfit[0] - nEntryMax;
                            break;
                        }
                    }
                    else
                    {
                        if (oBar.High >= oSignal.StopLoss)
                        {
                            nProfit = nEntryMin - oSignal.StopLoss;
                            break;
                        }
                        if (oBar.Low <= oSignal.TakeProfit[0])
                        {
                            nProfit = nEntryMin - oSignal.TakeProfit[0];
                            break;
                        }
                    }
                }

                nProfit *= nQtySl;
                if( nProfit < 0)
                {
                    nConsecutiveLosses++;
                    if (nConsecutiveLosses > nMaxConsecutiveLosses) nMaxConsecutiveLosses = nConsecutiveLosses;
                }
                else
                {
                    nTotalWon++;
                    nConsecutiveLosses = 0; // Reset on profit
                }
                nTotalProfit += nProfit;
            }

            Console.WriteLine($"Total profit: {nTotalProfit} USDT ({nTotalWon}/{nTotal})");
        */

        }

    }
}
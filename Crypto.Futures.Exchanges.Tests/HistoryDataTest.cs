using Crypto.Futures.Exchanges.Model;
using System.Globalization;
using System.Text;

namespace Crypto.Futures.Exchanges.Tests
{
    [TestClass]
    public class HistoryDataTest
    {
        [TestMethod]
        public async Task BarsTest()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(CommonTests.SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            DateTime dFrom = DateTime.Today.AddMonths(-2);

            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");


                IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
                Assert.IsNotNull(aSymbols, $"Symbols for {eType} should not be null.");
                IFuturesSymbol? oBtc = aSymbols.FirstOrDefault(p => p.Base == "BTC" && p.Quote == "USDT");
                IFuturesSymbol? oEth = aSymbols.FirstOrDefault(p => p.Base == "ETH" && p.Quote == "USDT");
                Assert.IsNotNull(oBtc);
                Assert.IsNotNull(oEth);

                IBar[]? aBars = await oExchange.History.GetBars(oBtc, BarTimeframe.H1, dFrom, DateTime.Today);
                Assert.IsNotNull(aBars);
                Assert.IsTrue(aBars.Length >= 450);

                IBar[]? aBarsMultiple = await oExchange.History.GetBars( new IFuturesSymbol[] {oEth, oBtc}, BarTimeframe.H1, dFrom, DateTime.Today);    
                Assert.IsNotNull(aBarsMultiple);
                Assert.IsTrue(aBarsMultiple.Length > 900);
            }

        }


        private class NewSymbolChance
        {
            public NewSymbolChance(IFuturesSymbol oSymbol) 
            { 
                Symbol = oSymbol;
            }

            public IFuturesSymbol Symbol { get; }

            public IBar? BarOpen { get; set; } = null;
            public IBar? BarClose { get; set; } = null;
            public decimal PriceSL { get; set; } = 0;
            public decimal PriceTp { get; set; } = 0;
            public decimal PriceOpen { get; set; } = 0;
            public decimal PriceClose { get; set; } = 0;
            public decimal Quantity {  get; set; } = 0; 
            public decimal Profit { get; set; } = 0;    
        }



        private void SaveBars(IBar[] aBars, IExchangeSetup oSetup )
        {
            string strFile = $"{oSetup.LogPath}/{aBars[0].Symbol.Symbol}_{aBars[0].Symbol.Exchange.ExchangeType.ToString()}.csv";


            StringBuilder oBuild = new StringBuilder();
            oBuild.AppendLine( "Date\tOpen\tHigh\tLow\tClose" );
            foreach (IBar bar in aBars)
            {
                string strDate = bar.DateTime.ToString("yyyy-MM-dd HH:mm");
                oBuild.Append($"{strDate}\t");
                oBuild.Append($"{bar.Open.ToString(CultureInfo.InvariantCulture)}\t");
                oBuild.Append($"{bar.High.ToString(CultureInfo.InvariantCulture)}\t");
                oBuild.Append($"{bar.Low.ToString(CultureInfo.InvariantCulture)}\t");
                oBuild.AppendLine($"{bar.Close.ToString(CultureInfo.InvariantCulture)}");
            }

            File.WriteAllText( strFile, oBuild.ToString() );    
        }


        [TestMethod]
        public async Task NewSymbolsBarTest()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(CommonTests.SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            DateTime dFrom = new DateTime(2025, 6, 1,0,0,0, DateTimeKind.Local);

            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();   
            List<IFuturesSymbol> aAllSymbols = new List<IFuturesSymbol>();  

            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");
                aExchanges.Add(oExchange);
                aAllSymbols.AddRange(oExchange.SymbolManager.GetAllValues());   
            }

            IFuturesSymbol[] aNew = aAllSymbols.Where(p=> p.ListDate >= dFrom && p.Quote == "USDT").OrderBy(p=> p.ListDate).ToArray(); 
            Assert.IsNotNull(aNew);
            Assert.IsTrue(aNew.Length > 10);

            IFuturesSymbol? oFound = aAllSymbols.FirstOrDefault(p => p.Base == "YBDBD");
            decimal nMoneyStart = 50;
            decimal nMoneyActual = nMoneyStart;
            decimal nRisk = 0.1M;

            decimal nPumpPercent = 50;
            int nMinutesMax = 120;
            List<NewSymbolChance> aChances = new List<NewSymbolChance>();

            int nWon = 0;
            int nTotal = 0;
            foreach( var oNew in aNew.Where(p=> p.Exchange.ExchangeType == ExchangeType.MexcFutures ) )
            {
                DateTime dList = oNew.ListDate.AddMinutes(-5);
                DateTime dTo = dList.AddDays(1);
                IBar[]? aBars = await oNew.Exchange.History.GetBars(oNew, BarTimeframe.M5, dList, dTo);  
                Assert.IsNotNull(aBars);
                IBar[] aCorrectBars = aBars.Where(p => p.DateTime.AddMinutes(5) >= oNew.ListDate).ToArray();
                Assert.IsTrue(aCorrectBars.Length > 5);

                // Pump bar
                decimal nOpenPrice = aCorrectBars[0].Open;
                decimal nPumpMin = (100.0M + nPumpPercent) * nOpenPrice / 100.0M;

                IBar? oPumpBar = aCorrectBars.FirstOrDefault(p => p.Close >= nPumpMin);
                if (oPumpBar == null) continue;

                int nMinutes = (int)(oPumpBar.DateTime - aCorrectBars[0].DateTime).TotalMinutes;
                if (nMinutes > nMinutesMax) continue;

                if (aCorrectBars[0].Open > aCorrectBars[0].Close && aCorrectBars[1].Open > aCorrectBars[1].Close) continue;

                SaveBars(aCorrectBars, oSetup);
                string strFound = $"{oNew.ToString()} - {oNew.ListDate.ToShortDateString()} {oNew.ListDate.ToShortTimeString()}";

                NewSymbolChance oChance = new NewSymbolChance(oNew);
                IBar? oOpenBar = aCorrectBars.Skip(1).FirstOrDefault( p=> p.Close < p.Open );
                if (oOpenBar == null) continue;
                oChance.BarOpen = oOpenBar;
                decimal nHigh = aCorrectBars.Where(p => p.DateTime <= oOpenBar.DateTime).Select(p => p.High).Max();
                oChance.PriceOpen = oOpenBar.Close;
                oChance.PriceSL = nHigh; // + 0.1M * oChance.PriceOpen;
                decimal nDiff = oChance.PriceSL - oChance.PriceOpen;
                oChance.PriceTp = oChance.PriceOpen - nDiff;

                decimal nMoneyRisk = (nMoneyActual * nRisk);
                if (nDiff <= 0) continue;
                decimal nQuantity = Math.Truncate(nMoneyRisk / nDiff);
                if (nQuantity <= 0) continue;
                oChance.Quantity = nQuantity;

                IBar? oBarSl = aCorrectBars.FirstOrDefault(p => p.DateTime > oOpenBar.DateTime && p.High >= oChance.PriceSL);
                IBar? oBarTp = aCorrectBars.FirstOrDefault(p => p.DateTime > oOpenBar.DateTime && p.Low <= oChance.PriceTp);

                if( oBarSl == null )
                {
                    if (oBarTp == null) continue;
                    oChance.BarClose = oBarTp;
                    oChance.PriceClose = oChance.PriceTp;
                }
                else
                {
                    if( oBarTp == null )
                    {
                        oChance.BarClose = oBarSl;
                        oChance.PriceClose = oChance.PriceSL;
                    }
                    else
                    {
                        if( oBarTp.DateTime < oBarSl.DateTime )
                        {
                            oChance.BarClose = oBarTp;
                            oChance.PriceClose = oChance.PriceTp;
                        }
                        else
                        {
                            oChance.BarClose = oBarSl;
                            oChance.PriceClose = oChance.PriceSL;
                        }
                    }

                }
                oChance.Profit = (oChance.PriceOpen - oChance.PriceClose) * oChance.Quantity;
                nMoneyActual += oChance.Profit;
                nTotal++;
                if (oChance.Profit > 0) nWon++;
                aChances.Add(oChance);
            }

            Console.WriteLine($"{nMoneyActual}");
        }
    }
}

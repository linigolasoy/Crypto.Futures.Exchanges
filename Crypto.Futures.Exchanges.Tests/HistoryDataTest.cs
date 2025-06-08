using Crypto.Futures.Exchanges.Model;

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


        [TestMethod]
        public async Task NewSymbolsBarTest()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(CommonTests.SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            DateTime dFrom = DateTime.Today.AddMonths(-1);

            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();   
            List<IFuturesSymbol> aAllSymbols = new List<IFuturesSymbol>();  

            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");
                aExchanges.Add(oExchange);
                aAllSymbols.AddRange(oExchange.SymbolManager.GetAllValues());   
            }

            IFuturesSymbol[] aNew = aAllSymbols.Where(p=> p.ListDate >= dFrom).OrderByDescending(p=> p.ListDate).ToArray(); 
            Assert.IsNotNull(aNew);
            Assert.IsTrue(aNew.Length > 10);

            foreach( var oNew in aNew )
            {
                DateTime dList = oNew.ListDate.AddMinutes(-15);
                DateTime dTo = dList.AddDays(1);
                IBar[]? aBars = await oNew.Exchange.History.GetBars(oNew, BarTimeframe.M15, dList, dTo);  
                Assert.IsNotNull(aBars);
                IBar[] aCorrectBars = aBars.Where(p => p.DateTime.AddMinutes(15) >= oNew.ListDate).ToArray();
                Assert.IsTrue(aCorrectBars.Length > 5);
                if (aCorrectBars[0].Open > aCorrectBars[0].Close && aCorrectBars[1].Open > aCorrectBars[1].Close) continue;
                Console.WriteLine("Founs");
            }

        }
    }
}

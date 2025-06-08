using Crypto.Futures.Exchanges.Model;

namespace Crypto.Futures.Exchanges.Tests
{
    [TestClass]
    public sealed class MarketDataTests 
    {
        private static string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";


        [TestMethod]
        public void MarketSymbolsTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            DateTime dFrom = DateTime.Today.AddMonths(-2);
            int nTotal = 0;

            foreach( ExchangeType eType in oSetup.ExchangeTypes )
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");


                IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
                Assert.IsNotNull(aSymbols, $"Symbols for {eType} should not be null.");
                Assert.IsTrue(aSymbols.Length > 50, $"There should be at least 50 symbol for {eType} exchange.");

                int nCount = aSymbols.Where(p=> p.ListDate >= dFrom).Count();   
                nTotal += nCount;
            }
            Assert.IsTrue(nTotal > 20);

        }

        [TestMethod]
        public async Task MarketFundingRatesTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");


            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
                Assert.IsNotNull(aSymbols, $"Symbols for {eType} should not be null.");

                IFuturesSymbol? oBtc = aSymbols.FirstOrDefault(x => x.Base == "BTC");
                Assert.IsNotNull(oBtc, $"BTC symbol should not be null for {eType} exchange.");
                IFuturesSymbol? oEth = aSymbols.FirstOrDefault(x => x.Base == "ETH");
                Assert.IsNotNull(oEth, $"ETH symbol should not be null for {eType} exchange.");

                IFundingRate? oBtcFunding = await oExchange.Market.GetFundingRate(oBtc);
                Assert.IsNotNull(oBtcFunding, $"BTC funding rate should not be null for {eType} exchange.");

                IFundingRate[]? aFundingRates = await oExchange.Market.GetFundingRates(new IFuturesSymbol[] {oBtc, oEth});
                Assert.IsNotNull(aFundingRates, $"Funding rates for BTC and ETH should not be null for {eType} exchange.");
                Assert.IsTrue(aFundingRates.Length > 1, $"There should be at least 2 funding rates for {eType} exchange.");
            }

        }


        [TestMethod]
        public async Task GetBestFundingRateArbitrageTest()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");


            // List of all funding rates in exchanges

            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            List<IFundingRate> aAllFundingRates = new List<IFundingRate>(); 
            foreach( ExchangeType eType in oSetup.ExchangeTypes )
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                aExchanges.Add(oExchange);
                IFundingRate[]? aFunding = await oExchange.Market.GetFundingRates();
                Assert.IsNotNull(aFunding, $"Funding rates for {eType} should not be null for exchange.");
                Assert.IsTrue(aFunding.Length > 50);
                aAllFundingRates.AddRange(aFunding);    
            }

            // Dictionary of different currencies
            Dictionary<string, List<IFundingRate>> aCurrencies = new Dictionary<string, List<IFundingRate>>();
            List<DateTime> aDates = new List<DateTime>();
            foreach( var oFunding in aAllFundingRates )
            {
                if (oFunding.Symbol.Quote != "USDT") continue;
                if( !aCurrencies.ContainsKey(oFunding.Symbol.Base) )
                {
                    aCurrencies.Add(oFunding.Symbol.Base, new List<IFundingRate>() { oFunding });
                }
                else
                {
                    aCurrencies[oFunding.Symbol.Base].Add(oFunding);
                }
                if( !aDates.Contains(oFunding.Next) ) aDates.Add(oFunding.Next);
            }
            Dictionary<string, IFundingRate[]> aCorrectCurrencies = new Dictionary<string, IFundingRate[]>();
            foreach (string strKey in aCurrencies.Keys)
            {
                if (aCurrencies[strKey].Count < 2) continue;
                aCorrectCurrencies.Add(strKey, aCurrencies[strKey].ToArray());  
            }

            DateTime dMin = aDates.Min();
            decimal nMax = -10;
            int nFound = 0;
            foreach( string strKey in aCorrectCurrencies.Keys)
            {
                IFundingRate[] aInKey = aCorrectCurrencies[strKey];
                IFundingRate[] aFound = aInKey.Where(p=> p.Next == dMin).ToArray();   
                if( aFound.Length <= 0 ) continue;
                nFound++;   
                for( int i = 0; i < aFound.Length; i++ )
                {
                    IFundingRate oRate1 = aFound[i];
                    if (aFound.Length == 1)
                    {
                        decimal nDiff = Math.Abs(oRate1.Rate);
                        if( nDiff > nMax )
                        {
                            nMax = nDiff;
                        }
                    }
                    else
                    {
                        for (int j = i + 1; j < aFound.Length; j++)
                        {
                            IFundingRate oRate2 = aFound[j];
                            decimal nDiff = Math.Abs(oRate1.Rate - oRate2.Rate);
                            if (nDiff > nMax)
                            {
                                nMax = nDiff;
                            }
                        }

                    }
                }

            }

            return;
        }
    }
}

using Crypto.Futures.Bot;
using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;

namespace Crypto.Futures.Exchanges.Tests
{
    [TestClass]
    public sealed class BotTests
    {
        private static string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";



        /// <summary>
        /// Create exchange instances for testing
        /// </summary>
        /// <param name="oSetup"></param>
        /// <returns></returns>
        private static IFuturesExchange[] CreateExchanges(IExchangeSetup oSetup)
        {
            ExchangeType[] aTypes = new ExchangeType[] {
                ExchangeType.BingxFutures,
                ExchangeType.BitgetFutures,
                ExchangeType.BitmartFutures
                // ,ExchangeType.CoinExFutures
            };

            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            foreach (ExchangeType eType in aTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");
                aExchanges.Add(oExchange);
            }
            return aExchanges.ToArray();
        }



        /// <summary>
        /// Test quoter price retrieval across exchanges
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task QuoterPricesTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            IFuturesExchange[] aExchanges = CreateExchanges(oSetup);
            IQuoter oQuoter = BotFactory.CreateQuoter(aExchanges);
            Assert.IsNotNull(oQuoter, "Quoter should not be null.");


            // List<IFuturesSymbol> aXrp = new List<IFuturesSymbol>();
            foreach( var oExchange in oQuoter.Exchanges )
            {
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"XRPUSDT symbol should exist on {oExchange.ExchangeType}.");
                decimal? nLongPrice = await oQuoter.GetLongPrice(oSymbol, 100);
                decimal? nShortPrice = await oQuoter.GetShortPrice(oSymbol, 100);
                Assert.IsNotNull(nLongPrice, $"Long price for {oExchange.ExchangeType} XRPUSDT should not be null.");
                Assert.IsNotNull(nShortPrice, $"Short price for {oExchange.ExchangeType} XRPUSDT should not be null.");

                nLongPrice = await oQuoter.GetLongPrice(oSymbol);
                nShortPrice = await oQuoter.GetShortPrice(oSymbol);
                Assert.IsNotNull(nLongPrice, $"Long price for {oExchange.ExchangeType} XRPUSDT should not be null.");
                Assert.IsNotNull(nShortPrice, $"Short price for {oExchange.ExchangeType} XRPUSDT should not be null.");
            }

        }



        /// <summary>
        /// Test quoter quantity matching across exchanges
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task QuoterQuantityMatchTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            IFuturesExchange[] aExchanges = CreateExchanges(oSetup);
            IQuoter oQuoter = BotFactory.CreateQuoter(aExchanges);
            Assert.IsNotNull(oQuoter, "Quoter should not be null.");

            Dictionary<string, List<IFuturesSymbol>> aSymbolBases = new Dictionary<string, List<IFuturesSymbol>>();
            // List<IFuturesSymbol> aXrp = new List<IFuturesSymbol>();
            foreach (var oExchange in oQuoter.Exchanges)
            {
                IFuturesSymbol[] aUsdt = oExchange.SymbolManager.GetAllValues().Where(p => p.Quote == "USDT").ToArray();    
                foreach (var oSymbol in aUsdt)
                {
                    if (!aSymbolBases.ContainsKey(oSymbol.Base))
                    {
                        aSymbolBases[oSymbol.Base] = new List<IFuturesSymbol>();
                    }
                    aSymbolBases[oSymbol.Base].Add(oSymbol);
                }
            }


            foreach( var oPair in aSymbolBases)
            {
                if (oPair.Value.Count < 2) continue;
                bool bQtyDecimals = (oPair.Value.Select(p => p.QuantityDecimals).Distinct().ToArray().Length > 1);
                bool bContract = (oPair.Value.Select(p => p.ContractSize).Distinct().ToArray().Length > 1);
                decimal nTestPrice = 3.78m;
                decimal nTestMoney = 31000m;
                decimal? nQty = oQuoter.GetBestQuantity(oPair.Value.ToArray(), nTestPrice, nTestMoney);
                Assert.IsNotNull(nQty, $"Best quantity for {oPair.Key} should not be null.");


            }
        }


        /// <summary>
        /// Test quoter quantity matching across exchanges
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task AccountWatcherTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            IFuturesExchange[] aExchanges = CreateExchanges(oSetup);

            IAccountWatcher oWatcher = BotFactory.CreateAccountWatcher(aExchanges);
            Assert.IsNotNull(oWatcher, "Account watcher should not be null.");


            await oWatcher.Start();

            // Quoter
            IQuoter oQuoter = BotFactory.CreateQuoter(aExchanges);
            Assert.IsNotNull(oQuoter, "Quoter should not be null.");

            await Task.Delay(2000);

            // Start balances retrieval 
            IBalance[] aStartBalances = oWatcher.GetBalances();
            Assert.IsNotNull(aStartBalances, "Start balances should not be null.");
            Assert.IsTrue(aStartBalances.Length  == oWatcher.Exchanges.Length, "Start balances should not be empty.");

            foreach( var oExchange in aExchanges)
            {
                IBalance? oStart = aStartBalances.FirstOrDefault(p => p.Exchange.ExchangeType == oExchange.ExchangeType);
                Assert.IsNotNull(oStart, $"Start balance for {oExchange.ExchangeType} should not be null.");

                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"XRPUSDT symbol should exist on {oExchange.ExchangeType}.");

                decimal? nLongPrice = await oQuoter.GetLongPrice(oSymbol, 1);
                Assert.IsNotNull(nLongPrice, $"Long price for {oExchange.ExchangeType} XRPUSDT should not be null.");

                // decimal? nQuantity = oQuoter.GetBestQuantity(new IFuturesSymbol[] { oSymbol }, nLongPrice.Value * 1.3M, 5m);
                // Assert.IsNotNull(nQuantity, $"Best quantity for {oExchange.ExchangeType} XRPUSDT should not be null.");

                // Place order
                string? strOrder = await oExchange.Trading.CreateOrder(oSymbol, true, 3, nLongPrice.Value * 0.8m);
                Assert.IsNotNull(strOrder, $"Order creation for {oExchange.ExchangeType} XRPUSDT should not be null.");
            }


            await oWatcher.Stop();  

        }


    }
}
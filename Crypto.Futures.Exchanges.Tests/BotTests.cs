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
                ExchangeType.BitmartFutures,
                ExchangeType.Hyperliquidity
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
            int nBalanceChanges = 0;    
            int nOrderChanges = 0;
            int nPositionChanges = 0;
            IPosition? oSavedPosition = null;

            oWatcher.OnBalanceChange += (IBalance oBalance) =>
            {
                nBalanceChanges++;
                Console.WriteLine($"Balance change on {oBalance.Exchange.ExchangeType}: {oBalance.Currency} {oBalance.Balance}");
            };
            oWatcher.OnPositionChange += (IPosition oPosition) =>
            {
                nPositionChanges++;
                oSavedPosition = oPosition;
                Console.WriteLine($"Position change on {oPosition.Symbol.Exchange.ExchangeType}: {oPosition.Symbol.Base}{oPosition.Symbol.Quote} {oPosition.Quantity} ");
            };

            oWatcher.OnOrderChange += (IOrder oOrder) =>
            {
                nOrderChanges++;
                Console.WriteLine($"Order change on {oOrder.Symbol.Exchange.ExchangeType}: {oOrder.Symbol.Base}{oOrder.Symbol.Quote} {oOrder.Quantity} @ {oOrder.Price} Status: {oOrder.Status}");
            };

            await oWatcher.Start();

            // Quoter
            IQuoter oQuoter = BotFactory.CreateQuoter(aExchanges);
            Assert.IsNotNull(oQuoter, "Quoter should not be null.");

            await Task.Delay(2000);

            // Start balances retrieval 
            IBalance[] aStartBalances = oWatcher.GetBalances();
            Assert.IsNotNull(aStartBalances, "Start balances should not be null.");
            Assert.IsTrue(aStartBalances.Length  == oWatcher.Exchanges.Length, "Start balances should not be empty.");

            decimal nQuantity = 8;
            foreach( var oExchange in aExchanges ) //.Where(p=> p.ExchangeType == ExchangeType.Hyperliquidity))
            {
                int nOrderChangesActual = nOrderChanges;
                IBalance? oStart = aStartBalances.FirstOrDefault(p => p.Exchange.ExchangeType == oExchange.ExchangeType);
                Assert.IsNotNull(oStart, $"Start balance for {oExchange.ExchangeType} should not be null.");

                decimal nTotal = oStart.Balance;  

                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"XRPUSDT symbol should exist on {oExchange.ExchangeType}.");

                decimal? nLongPrice = await oQuoter.GetLongPrice(oSymbol, nQuantity);
                Assert.IsNotNull(nLongPrice, $"Long price for {oExchange.ExchangeType} XRPUSDT should not be null.");

                // decimal? nQuantity = oQuoter.GetBestQuantity(new IFuturesSymbol[] { oSymbol }, nLongPrice.Value * 1.3M, 5m);
                // Assert.IsNotNull(nQuantity, $"Best quantity for {oExchange.ExchangeType} XRPUSDT should not be null.");

                decimal nPriceLimit = Math.Round( nLongPrice.Value * 0.8m, oSymbol.Decimals);

                // Place order
                string? strOrder = await oExchange.Trading.CreateOrder(oSymbol, true, nQuantity, nPriceLimit);
                Assert.IsNotNull(strOrder, $"Order creation for {oExchange.ExchangeType} XRPUSDT should not be null.");


                await Task.Delay(2000);

                bool bClosedOrder = await oExchange.Trading.CloseOrders(oSymbol);
                await Task.Delay(2000);
                Assert.IsTrue(bClosedOrder, $"Order closing for {oExchange.ExchangeType} XRPUSDT should be successful.");
                int OrderChangesDiff = nOrderChanges - nOrderChangesActual;
                Assert.IsTrue(OrderChangesDiff == 2, $"Order changes should be detected for {oExchange.ExchangeType} XRPUSDT, changes detected: {OrderChangesDiff}");


                // Place Market order
                nOrderChangesActual = nOrderChanges;
                int nPositionChangesActual = nPositionChanges;  
                oSavedPosition = null;
                strOrder = await oExchange.Trading.CreateOrder(oSymbol, true, nQuantity);
                Assert.IsNotNull(strOrder, $"Order creation for {oExchange.ExchangeType} XRPUSDT should not be null.");


                await Task.Delay(3000);
                Assert.IsNotNull(oSavedPosition, $"Position change should be detected for {oExchange.ExchangeType} XRPUSDT after market order.");   

                string? strCloseOrder = await oExchange.Trading.ClosePosition(oSavedPosition);
                Assert.IsNotNull(strCloseOrder, $"Position closing for {oExchange.ExchangeType} XRPUSDT should be successful.");

                await Task.Delay(3000);
                Assert.IsTrue(!oSavedPosition.IsOpen, $"Position should be closed for {oExchange.ExchangeType} XRPUSDT after close order.");

                OrderChangesDiff = nOrderChanges - nOrderChangesActual;
                int PositionChangesDiff = nPositionChanges - nPositionChangesActual;
                Assert.IsTrue(OrderChangesDiff >= 2, $"Order changes should be detected for {oExchange.ExchangeType} XRPUSDT, changes detected: {OrderChangesDiff}");
                Assert.IsTrue(PositionChangesDiff == 2, $"Position changes should be detected for {oExchange.ExchangeType} XRPUSDT, changes detected: {PositionChangesDiff}");

            }


            await oWatcher.Stop();  

        }


        /// <summary>
        /// Test quoter quantity matching across exchanges
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TraderTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            IFuturesExchange[] aExchanges = CreateExchanges(oSetup);

            IAccountWatcher oWatcher = BotFactory.CreateAccountWatcher(aExchanges);
            await oWatcher.Start();
            await Task.Delay(2000);

            IQuoter oQuoter = BotFactory.CreateQuoter(aExchanges);
            Assert.IsNotNull(oQuoter, "Quoter should not be null.");
            ICommonLogger oLogger = ExchangeFactory.CreateLogger(oSetup, "TraderTests");

            ICryptoTrader oTrader = BotFactory.CreateTrader(oSetup, oLogger, oWatcher, oQuoter);
            Assert.IsNotNull(oTrader, "Trader should not be null.");

            decimal nQuantity = 8;

            foreach (var oExchange in aExchanges)
            {
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"XRPUSDT symbol should exist on {oExchange.ExchangeType}.");
                decimal? nLongPrice = await oQuoter.GetLongPrice(oSymbol, nQuantity);

                Assert.IsNotNull(nLongPrice, $"Long price for {oExchange.ExchangeType} XRPUSDT should not be null.");
                decimal nPriceLimit = Math.Round(nLongPrice.Value * 0.8m, oSymbol.Decimals);
                // Put leverage
                bool bLeverage = await oTrader.PutLeverage(oSymbol);
                Assert.IsTrue(bLeverage, $"Leverage setting for {oExchange.ExchangeType} XRPUSDT should be successful.");


                // Place limit order test
                IOrder? oOrder = await oTrader.CreateLimitOrder(oSymbol, true, nQuantity, nPriceLimit);
                Assert.IsNotNull(oOrder, $"Order creation for {oExchange.ExchangeType} XRPUSDT should not be null.");
                await Task.Delay(2000);
                Assert.IsTrue(oTrader.AccountWatcher.GetOrders().Length > 0);
                bool bClosedOrder = await oTrader.ClosePendingOrders(oSymbol);
                await Task.Delay(2000);
                Assert.IsTrue(bClosedOrder, $"Order closing for {oExchange.ExchangeType} XRPUSDT should be successful.");
                Assert.IsTrue(oOrder.Status == ModelOrderStatus.Canceled, $"Order should be cancelled for {oExchange.ExchangeType} XRPUSDT.");

                // Market order test
                IPosition? oPosition = await oTrader.Open(oSymbol, true, nQuantity);
                Assert.IsNotNull(oPosition, $"Position opening for {oExchange.ExchangeType} XRPUSDT should not be null.");

                await Task.Delay(2000);

                decimal? nProfit = await oTrader.Quoter.GetProfit(oPosition );
                Assert.IsNotNull(nProfit, $"Profit retrieval for {oExchange.ExchangeType} XRPUSDT should not be null.");

                bool bUpdated = await oTrader.UpdatePositions();
                Assert.IsTrue(bUpdated, $"Position update for {oExchange.ExchangeType} XRPUSDT should be successful.");


                bool bClosed = await oTrader.Close(oPosition);
                Assert.IsTrue(bClosed, $"Position closing for {oExchange.ExchangeType} XRPUSDT should be successful.");
                Assert.IsTrue(!oPosition.IsOpen, $"Position closing for {oExchange.ExchangeType} XRPUSDT should be successful.");

            }

            await oWatcher.Stop();
        }

    }
}
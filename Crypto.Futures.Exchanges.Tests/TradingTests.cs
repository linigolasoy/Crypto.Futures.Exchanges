using Crypto.Futures.Bot;
using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Tests
{
    [TestClass]
    public sealed class TradingTests
    {
        private static string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";


        [TestMethod]
        public async Task CloseOrdersTest()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            string strCurrency = "XRP";
            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == strCurrency && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"Symbol for {strCurrency}USDT should not be null.");

                bool bCloseOrders = await oExchange.Trading.CloseOrders(oSymbol);
                Assert.IsTrue(bCloseOrders, "Close orders should be successful.");
            }
        }


        [TestMethod]
        public async Task SimpleMarketTests()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            string strCurrency = "XRP";
            decimal nMoney = 2;
            decimal nLeverage = 10;
            decimal nQuantity = 5;
            foreach (ExchangeType eType in oSetup.ExchangeTypes.Where(p => p == ExchangeType.CoinExFutures))
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;
                IFuturesSymbol? oSymbolContract = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Quote == "USDT" && p.ContractSize != 1);
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == strCurrency && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"Symbol for {strCurrency}USDT should not be null.");

                bool bLeverage = await oExchange.Account.SetLeverage(oSymbol, nLeverage);
                Assert.IsTrue(bLeverage, "Setting leverage should be successful.");



                decimal nInvest = nMoney * nLeverage;
                bool bOrderResult = await oExchange.Trading.CreateOrder(oSymbol, false, nQuantity);
                Assert.IsTrue(bOrderResult, "Order data should be true (order created).");

                await Task.Delay(1000); // Wait for order to be processed
                IPosition[]? aPositions = await oExchange.Account.GetPositions();
                Assert.IsNotNull(aPositions, "Positions should not be null.");
                Assert.IsTrue(aPositions.Length > 0, "There should be at least one position.");
                IPosition? oPosition = aPositions.FirstOrDefault(p => p.Symbol.Symbol.Equals(oSymbol.Symbol));
                Assert.IsNotNull(oPosition, "Position for the symbol should not be null.");

                bool bCloseResult = await oExchange.Trading.ClosePosition(oPosition);
                Assert.IsTrue(bCloseResult, "Close position data should be true (position closed).");

                aPositions = await oExchange.Account.GetPositions();
                Assert.IsNotNull(aPositions, "Positions should not be null after closing.");
                oPosition = aPositions.FirstOrDefault(p => p.Symbol.Symbol.Equals(oSymbol.Symbol));
                Assert.IsNull(oPosition, "Position for the symbol should be null after closing.");
            }

        }



        [TestMethod]
        public async Task BotTradingTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");
            ICommonLogger oLogger = ExchangeFactory.CreateLogger(oSetup, "TestBotLogger");

            ITradingBot oBot = BotFactory.CreateArbitrageBot(oSetup, oLogger, false);
            string strCurrency = "XRP";
            decimal nQuantity = 5;

            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;

                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == strCurrency && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"Symbol for {strCurrency}USDT should not be null.");

                bool bLeverage = await oBot.Trader.PutLeverage(oSymbol);
                Assert.IsTrue(bLeverage, "Setting leverage should be successful.");

                ITraderPosition? oPosition = await oBot.Trader.Open(oSymbol, true, nQuantity);
                Assert.IsNotNull(oPosition, "Position should not be null after opening.");

                oPosition.Update();

                Assert.IsTrue(oPosition.IsLong, "Position should be long.");
                bool bCloseResult = await oBot.Trader.Close(oPosition);
                Assert.IsTrue(bCloseResult, "Close position data should be true (position closed).");
            }
        }
    }
}
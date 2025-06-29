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
        public async Task SimpleMarketTests()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            string strCurrency = "XRP";
            decimal nMoney = 2;
            decimal nLeverage = 10;
            decimal nQuantity = 5;
            foreach ( ExchangeType eType in oSetup.ExchangeTypes.Where(p=> p == ExchangeType.MexcFutures) )
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                // if (!oExchange.Tradeable) continue;
                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == strCurrency && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"Symbol for {strCurrency}USDT should not be null.");

                bool bLeverage = await oExchange.Account.SetLeverage(oSymbol, nLeverage);
                Assert.IsTrue(bLeverage, "Setting leverage should be successful.");

                decimal nInvest = nMoney * nLeverage;
                bool bOrderResult = await oExchange.Trading.CreateOrder(oSymbol, nLeverage, true, nQuantity );
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
        public async Task LeverageTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            decimal nNewLeverage = 15;
            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;

                IFuturesSymbol? oBtc = oExchange.SymbolManager.GetAllValues().First(p => p.Base == "ETH" && p.Quote == "USDT");
                Assert.IsNotNull(oBtc);

                decimal? nLeverage = await oExchange.Account.GetLeverage(oBtc);
                Assert.IsNotNull(nLeverage);

                bool bSet = await oExchange.Account.SetLeverage(oBtc, nNewLeverage);
                Assert.IsTrue(bSet);

            }

        }


        [TestMethod]
        public async Task PositionTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;

                IPosition[]? aPositions = await oExchange.Account.GetPositions();
                Assert.IsNotNull(aPositions);


            }

        }

    }
}

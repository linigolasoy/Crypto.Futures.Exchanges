using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Tests
{
    [TestClass]
    public sealed class AccountTests 
    {
        private static string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";


        [TestMethod]
        public async Task BalancesTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            foreach( ExchangeType eType in oSetup.ExchangeTypes )
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;

                IBalance[]? aBalances = await oExchange.Account.GetBalances();
                Assert.IsNotNull(aBalances);
                Assert.IsTrue(aBalances.Length > 0);

            }

        }


        [TestMethod]
        public async Task LeverageTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            decimal nNewLeverage = 10;
            foreach (ExchangeType eType in oSetup.ExchangeTypes.Where(p=> p == ExchangeType.CoinExFutures))
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;

                IFuturesSymbol? oBtc = oExchange.SymbolManager.GetAllValues().First(p => p.Base == "USTC" && p.Quote == "USDT");
                Assert.IsNotNull(oBtc);

                decimal? nLeverage = await oExchange.Account.GetLeverage(oBtc);
                Assert.IsNotNull(nLeverage);

                bool bSet = await oExchange.Account.SetLeverage(oBtc, nNewLeverage);
                Assert.IsTrue(bSet);

            }

        }



        [TestMethod]
        public async Task PrivateSocketsTests()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            foreach (ExchangeType eType in oSetup.ExchangeTypes.Where(p => p == ExchangeType.BingxFutures))
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                if (!oExchange.Tradeable) continue;

                Dictionary<WsMessageType, int> aReceived = new Dictionary<WsMessageType, int>();
                aReceived[WsMessageType.Balance] = 0;
                aReceived[WsMessageType.Order] = 0;
                aReceived[WsMessageType.Position] = 0;

                IWebsocketPrivate oPrivate = oExchange.Account.WebsocketPrivate;

                IOrder? oLastOrder = null;
                IPosition? oLastPosition = null;
                
                oPrivate.OnOrder += (p) => { aReceived[WsMessageType.Order]++; oLastOrder = p; };
                oPrivate.OnBalance += (p) => { aReceived[WsMessageType.Balance]++; };
                oPrivate.OnPosition += (p) => { aReceived[WsMessageType.Position]++; oLastPosition = p; };
                bool bStarted = await oPrivate.Start(); 
                Assert.IsTrue(bStarted);

                IFuturesSymbol? oXrp = oExchange.SymbolManager.GetAllValues().First(p => p.Base == "XRP" && p.Quote == "USDT");
                Assert.IsNotNull(oXrp);

                ITicker[]? aTickers = await oExchange.Market.GetTickers();
                Assert.IsNotNull(aTickers);
                ITicker? oFound = aTickers.FirstOrDefault(p=> p.Symbol.Symbol == oXrp.Symbol);   
                Assert.IsNotNull(oFound);
                decimal nPrice = Math.Round( oFound.LastPrice * 0.8M, oXrp.Decimals);
                await Task.Delay(2000);
                // Limit order test
                bool bOrdered = await oExchange.Trading.CreateOrder(oXrp, true, 5, nPrice);
                Assert.IsTrue(bOrdered);
                await Task.Delay(2000);
                Assert.IsNotNull( oLastOrder );
                Assert.IsTrue(oLastOrder.Status == ModelOrderStatus.New || oLastOrder.Status == ModelOrderStatus.Placed);
                bool bCancel = await oExchange.Trading.CloseOrders(oXrp);
                await Task.Delay(2000); 
                Assert.IsTrue(oLastOrder != null);
                Assert.IsTrue(oLastOrder.Status == ModelOrderStatus.Canceled);

                // Market order test
                bOrdered = await oExchange.Trading.CreateOrder(oXrp, true, 5);
                Assert.IsTrue(bOrdered);
                await Task.Delay(2000);
                Assert.IsTrue(oLastOrder != null);
                Assert.IsTrue(oLastOrder.Status == ModelOrderStatus.Filled);

                Assert.IsNotNull(oLastPosition);

                bool bClosePosition = await oExchange.Trading.ClosePosition(oLastPosition);
                Assert.IsTrue(bClosePosition);
                await Task.Delay(2000);
                Assert.IsTrue(!oLastPosition.IsOpen);

                /*
                bool bClosed = await oExchange.Trading.ClosePosition(oLastPosition);
                Assert.IsTrue(bClosed);
                await Task.Delay(1000);
                Assert.IsTrue(oLastPosition.PriceClose != null);
                await Task.Delay(1000);
                */

                Assert.IsTrue(oPrivate.Positions.Length <= 0);

                bool bEnded = await oPrivate.Stop();
                
                Assert.IsTrue(aReceived[WsMessageType.Balance] > 0);
                Assert.IsTrue(aReceived[WsMessageType.Order] > 0);
                Assert.IsTrue(aReceived[WsMessageType.Position] > 0);
                Assert.IsTrue(bEnded);

            }

        }

    }
}

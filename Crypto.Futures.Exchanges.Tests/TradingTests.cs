using Crypto.Futures.Bot;
using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.Arbitrage;
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
                string? strOrderId = await oExchange.Trading.CreateOrder(oSymbol, false, nQuantity);
                Assert.IsNotNull(strOrderId);

                await Task.Delay(1000); // Wait for order to be processed
                IPosition[]? aPositions = await oExchange.Account.GetPositions();
                Assert.IsNotNull(aPositions, "Positions should not be null.");
                Assert.IsTrue(aPositions.Length > 0, "There should be at least one position.");
                IPosition? oPosition = aPositions.FirstOrDefault(p => p.Symbol.Symbol.Equals(oSymbol.Symbol));
                Assert.IsNotNull(oPosition, "Position for the symbol should not be null.");

                string? strCloseOrder = await oExchange.Trading.ClosePosition(oPosition);
                Assert.IsNotNull(strCloseOrder);

                aPositions = await oExchange.Account.GetPositions();
                Assert.IsNotNull(aPositions, "Positions should not be null after closing.");
                oPosition = aPositions.FirstOrDefault(p => p.Symbol.Symbol.Equals(oSymbol.Symbol));
                Assert.IsNull(oPosition, "Position for the symbol should be null after closing.");
            }

        }

        private IQuoter CreateQuoter(IExchangeSetup oSetup, ICommonLogger oLogger)
        {
            ExchangeType[] aTypes = new ExchangeType[] {
                ExchangeType.BingxFutures,
                ExchangeType.BitgetFutures,
                ExchangeType.BitmartFutures,
                ExchangeType.CoinExFutures
            };

            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            foreach (ExchangeType eType in aTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType, oLogger);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");
                aExchanges.Add(oExchange);
            }
            IQuoter oQuoter = BotFactory.CreateQuoter(aExchanges.ToArray());
            return oQuoter;
        }


        [TestMethod]
        public async Task PaperTradingTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");
            ICommonLogger oLogger = ExchangeFactory.CreateLogger(oSetup, "PaperTradingTests");

            IQuoter oQuoter = CreateQuoter(oSetup, oLogger);

            ICryptoTrader? oTrader = BotFactory.CreateTrader(oSetup, oLogger, oQuoter.Exchanges, true);
            Assert.IsNotNull(oTrader, "Trader should not be null.");

            oTrader.InitBalances();
            await Task.Delay(1000); // Wait for balances to be initialized
            Assert.IsTrue(oTrader.Balances.Length >= 2, "There should be at least two balance.");

            // Create quoter

            foreach (var oBalance in oTrader.Balances)
            {
                Assert.IsTrue(oBalance.Avaliable >= oTrader.Money, "Each balance should have at least money needed for trader.");
                IFuturesSymbol? oSymbol = oBalance.Exchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == "XRP" && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, "Symbol for XRPUSDT should not be null.");
                bool bLeverage = await oTrader.PutLeverage(oSymbol);
                Assert.IsTrue(bLeverage, "Setting leverage should be successful.");



            }

        }


        [TestMethod]
        public async Task BotTradingTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");

            IArbitrageBot oBot = BotFactory.CreateArbitrageBot(oSetup, false);
            string strCurrency = "XRP";
            decimal nQuantity = 5;
            oBot.Trader.OrderTimeout = 30;
            foreach (var oExchange in oBot.Exchanges.Where(p=> p.ExchangeType == ExchangeType.BitmartFutures))
            {

                IFuturesSymbol? oSymbol = oExchange.SymbolManager.GetAllValues().FirstOrDefault(p => p.Base == strCurrency && p.Quote == "USDT");
                Assert.IsNotNull(oSymbol, $"Symbol for {strCurrency}USDT should not be null.");

                bool bStarted = await oExchange.Account.WebsocketPrivate.Start();
                Assert.IsTrue(bStarted);
                await Task.Delay(2000);

                IWebsocketSubscription? oSubscription = await oExchange.Market.Websocket.Subscribe(oSymbol, WsMessageType.OrderbookPrice);
                Assert.IsNotNull(oSubscription);

                await Task.Delay(2000);
                var oData = oExchange.Market.Websocket.DataManager.GetData(oSymbol);
                Assert.IsNotNull(oData);

                Assert.IsNotNull(oData.LastOrderbookPrice);

                bool bLeverage = await oBot.Trader.PutLeverage(oSymbol);
                Assert.IsTrue(bLeverage, "Setting leverage should be successful.");

                ICryptoPosition? oPosition = await oBot.Trader.OpenFillOrKill(oSymbol, true, nQuantity, oData.LastOrderbookPrice.AskPrice);
                if( oPosition == null )
                {
                    oPosition = await oBot.Trader.Open(oSymbol, true, nQuantity);
                }
                Assert.IsNotNull(oPosition, "Position should not be null after opening.");

                Assert.IsTrue(oPosition.IsLong, "Position should be long.");
                Assert.IsNotNull(oPosition.OrderOpen, "Order open should exist");
                Assert.IsTrue(oPosition.OrderOpen.FilledPrice > 0, "Filled price should be not zero");
                Assert.IsTrue(oPosition.IsLong, "Position should be long.");
                bool bCloseResult = await oBot.Trader.CloseFillOrKill(oPosition, oData.LastOrderbookPrice.BidPrice);
                if (!bCloseResult)
                {
                    bCloseResult = await oBot.Trader.CloseFillOrKill(oPosition);
                }
                Assert.IsTrue(bCloseResult, "Close position data should be true (position closed).");

                bool bStopped = await oExchange.Account.WebsocketPrivate.Stop();
                Assert.IsTrue(bStopped);
                await Task.Delay(1000);

            }
        }
    }
}
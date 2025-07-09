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



    }
}

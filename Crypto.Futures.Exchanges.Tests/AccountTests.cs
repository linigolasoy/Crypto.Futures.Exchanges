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
                if (eType == ExchangeType.BlofinFutures) continue;
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");

                IBalance[]? aBalances = await oExchange.Account.GetBalances();
                Assert.IsNotNull(aBalances);
                Assert.IsTrue(aBalances.Length > 0);

            }

        }



    }
}

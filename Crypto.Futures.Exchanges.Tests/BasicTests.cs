namespace Crypto.Futures.Exchanges.Tests
{
    [TestClass]
    public sealed class BasicTests
    {
        [TestMethod]
        public void LoadingSetupTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(CommonTests.SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");
            Assert.IsTrue(oSetup.ExchangeTypes.Length > 0, "There should be at least one exchange in the setup.");
            Assert.IsTrue(oSetup.ApiKeys.Length > 0, "There should be at least one API key in the setup.");

        }
    }
}

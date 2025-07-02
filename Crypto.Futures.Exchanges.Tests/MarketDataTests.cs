using Crypto.Futures.Exchanges.Factory;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;

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
                IFuturesSymbol[] aFilter2 = aSymbols.Where(p => p.Base == "LPT").ToArray();
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


            foreach (ExchangeType eType in oSetup.ExchangeTypes ) //.Where(p=> p== ExchangeType.BitunixFutures))
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");
                if( !oExchange.Tradeable) continue; // Skip non-tradeable exchanges
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
        public async Task MarketTickersTest()
        {

            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");


            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");


                IFuturesSymbol[]? aSymbols = oExchange.SymbolManager.GetAllValues();
                Assert.IsNotNull(aSymbols, $"Symbols for {eType} should not be null.");

                ITicker[]? aTickers = await oExchange.Market.GetTickers();
                Assert.IsNotNull(aTickers, $"Tikers for {eType} should not be null.");

                Assert.IsTrue(aTickers.Length >= aSymbols.Length / 2);

            }

        }


        [TestMethod]
        public async Task MarketSocketTests()
        {
            IExchangeSetup oSetup = ExchangeFactory.CreateSetup(SETUP_FILE);
            Assert.IsNotNull(oSetup, "Setup should not be null.");


            // List of all funding rates in exchanges

            List<IFuturesExchange> aExchanges = new List<IFuturesExchange>();
            foreach (ExchangeType eType in oSetup.ExchangeTypes)
            {
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                aExchanges.Add(oExchange);
            }


            string[] aCoins = new string[] { "BTC", "ETH", "XRP", "LTC", "SOL", "BNB", "ADA", "DOT", "DOGE", "TRX" };
            foreach (var oExchange in aExchanges.Where(p=> p.ExchangeType == ExchangeType.BingxFutures))
            {
                if( !oExchange.Tradeable) continue;
                IWebsocketPublic oWs = oExchange.Market.Websocket;
                IFuturesSymbol[] aFirst = oExchange.SymbolManager.GetAllValues().Where(p=> p.Quote.Equals("USDT")).Where( p=> aCoins.Contains(p.Base)).ToArray();   
                oWs.Timeframe = BarTimeframe.M15;
                bool bStarted = await oWs.Start();
                Assert.IsTrue(bStarted);


                foreach( var oSymbol in aFirst )
                {
                    var oSubscription = await oWs.Subscribe(oSymbol, WsMessageType.FundingRate);
                    Assert.IsNotNull(oSubscription, $"Subscription for {oSymbol.Symbol} should not be null.");
                    oSubscription = await oWs.Subscribe(oSymbol, WsMessageType.LastPrice);
                    Assert.IsNotNull(oSubscription, $"Subscription for {oSymbol.Symbol} should not be null.");
                    oSubscription = await oWs.Subscribe(oSymbol, WsMessageType.OrderbookPrice);
                    Assert.IsNotNull(oSubscription, $"Subscription for {oSymbol.Symbol} should not be null.");
                }

                await Task.Delay(25000);

                DateTime dNow = DateTime.Now;
                double nSeconds = (dNow - oWs.DataManager.LastUpdate).TotalSeconds;
                Assert.IsTrue(  nSeconds < 10 );
                await oWs.Stop();
                List<IFundingRate> aFunding = new List<IFundingRate>();
                List<ILastPrice> aLast = new List<ILastPrice>();
                List<IOrderbookPrice> aOrderbook = new List<IOrderbookPrice>();
                foreach (var oSymbol in aFirst)
                {
                    var oData = oWs.DataManager.GetData(oSymbol);
                    Assert.IsNotNull(oData);
                    if( oData.FundingRate != null ) aFunding.Add(oData.FundingRate);
                    if (oData.LastPrice != null) aLast.Add(oData.LastPrice);
                    if( oData.LastOrderbookPrice != null ) aOrderbook.Add(oData.LastOrderbookPrice);
                }
                Assert.IsTrue(aFunding.Count > 1);
                Assert.IsTrue(aLast.Count > 1);
                Assert.IsTrue(aOrderbook.Count > 1);



            }

        }
    }
}

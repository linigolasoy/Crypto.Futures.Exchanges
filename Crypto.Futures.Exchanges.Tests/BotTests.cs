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
    public sealed class BotTests
    {
        private static string SETUP_FILE = "D:/Data/CryptoFutures/FuturesSetup.json";


        private static IFuturesExchange[] CreateExchanges(IExchangeSetup oSetup)
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
                IFuturesExchange oExchange = ExchangeFactory.CreateExchange(oSetup, eType);
                Assert.IsNotNull(oExchange, $"Exchange for {eType} should not be null.");
                aExchanges.Add(oExchange);
            }
            return aExchanges.ToArray();
        }



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


    }
}
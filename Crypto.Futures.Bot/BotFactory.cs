using Crypto.Futures.Bot.Arbitrage;
using Crypto.Futures.Bot.FundingRateBot;
using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Bot.Interface.Arbitrage;
using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Bot.Model.ArbitrageTrading;
using Crypto.Futures.Bot.NewSymbols;
using Crypto.Futures.Exchanges;

namespace Crypto.Futures.Bot
{
    public class BotFactory
    {
        /*
        public static ITradingBot CreateNewSymbolBot( IExchangeSetup oSetup, ICommonLogger oLogger )
        {
            return new NewSymbolBot( oSetup, oLogger ); 
        }
        */

        /*
        public static ITradingBot CreateArbitrageBot(IExchangeSetup oSetup, ICommonLogger oLogger, bool bPaperTrading)
        {
            return new ArbitrageBot(oSetup, oLogger, bPaperTrading);
        }
        */

        public static IArbitrageBot CreateArbitrageBot(IExchangeSetup oSetup, bool bPaperTrading)
        {
            return new CryptoArbitrageBot(oSetup, bPaperTrading);
        }
        public static IFundingRateBot CreateFundingRateBot(IExchangeSetup oSetup, ICommonLogger oLogger, bool bPaperTrading)
        {
            return new FundingRateMultiExchangeBot(oSetup, oLogger, bPaperTrading);
        }

    }
}

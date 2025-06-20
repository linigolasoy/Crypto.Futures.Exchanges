using Crypto.Futures.Bot.Arbitrage;
using Crypto.Futures.Bot.NewSymbols;
using Crypto.Futures.Exchanges;

namespace Crypto.Futures.Bot
{
    public class BotFactory
    {
        public static ITradingBot CreateNewSymbolBot( IExchangeSetup oSetup, ICommonLogger oLogger )
        {
            return new NewSymbolBot( oSetup, oLogger ); 
        }

        public static ITradingBot CreateArbitrageBot(IExchangeSetup oSetup, ICommonLogger oLogger, bool bPaperTrading)
        {
            return new ArbitrageBot(oSetup, oLogger, bPaperTrading);
        }


    }
}

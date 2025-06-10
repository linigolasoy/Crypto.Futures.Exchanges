using Crypto.Futures.Exchanges;

namespace Crypto.Futures.Bot
{
    public class BotFactory
    {
        public static ITradingBot CreateNewSymbolBot( IExchangeSetup oSetup, ICommonLogger oLogger )
        {
            return new NewSymbolBot( oSetup, oLogger ); 
        }
    }
}

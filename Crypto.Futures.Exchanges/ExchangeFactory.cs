using Crypto.Futures.Exchanges.Model;

namespace Crypto.Futures.Exchanges
{
    public class ExchangeFactory
    {


        public static IExchangeSetup CreateSetup( string strFile )
        {
            string strContent = File.ReadAllText(strFile);
            return CreateSetupFromText(strContent);
        }

        public static IExchangeSetup CreateSetupFromText(string strText)
        {
            return new BaseSetup(strText);
        }

        public static ICommonLogger CreateLogger( IExchangeSetup oSetup, string strName )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create an exchange instance based on the provided setup and exchange type.
        /// </summary>
        /// <param name="oSetup"></param>
        /// <param name="eType"></param>
        /// <param name="oLogger"></param>
        /// <returns></returns>
        public static IFuturesExchange CreateExchange(IExchangeSetup oSetup, ExchangeType eType, ICommonLogger? oLogger = null)
        {
            switch( eType )
            {
                case ExchangeType.MexcFutures:
                    return new Mexc.MexcFutures(oSetup, oLogger);
                case ExchangeType.BingxFutures:
                    return new Bingx.BingxFutures(oSetup, oLogger);
                case ExchangeType.CoinExFutures:
                    return new Coinex.CoinexFutures(oSetup, oLogger);   
                case ExchangeType.BitgetFutures:
                    return new Bitget.BitgetFutures(oSetup, oLogger);
                case ExchangeType.BitmartFutures:
                    return new Bitmart.BitmartFutures(oSetup, oLogger);
                default:
                    break;
            }
            throw new NotImplementedException();
        }
    }
}

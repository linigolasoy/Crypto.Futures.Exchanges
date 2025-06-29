using Crypto.Futures.Exchanges.Bingx;
using Crypto.Futures.Exchanges.Bitget;
using Crypto.Futures.Exchanges.Bitmart;
using Crypto.Futures.Exchanges.Bitunix;
using Crypto.Futures.Exchanges.Coinex;
using Crypto.Futures.Exchanges.Mexc;
using Cypto.Futures.Exchanges.Blofin;

namespace Crypto.Futures.Exchanges.Factory
{
    public class ExchangeFactory
    {
        public static IExchangeSetup CreateSetup(string strFile)
        {
            string strContent = File.ReadAllText(strFile);
            return CreateSetupFromText(strContent);
        }

        public static IExchangeSetup CreateSetupFromText(string strText)
        {
            return new BaseSetup(strText);
        }

        public static ICommonLogger CreateLogger(IExchangeSetup oSetup, string strName)
        {
            return new CommonLogger(oSetup, strName, new CancellationTokenSource().Token);
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
            switch (eType)
            {
                case ExchangeType.BlofinFutures:
                    return new BlofinFutures(oSetup, oLogger);
                case ExchangeType.MexcFutures:
                    return new MexcFutures(oSetup, oLogger);
                case ExchangeType.BingxFutures:
                    return new BingxFutures(oSetup, oLogger);
                case ExchangeType.CoinExFutures:
                    return new CoinexFutures(oSetup, oLogger);
                case ExchangeType.BitgetFutures:
                    return new BitgetFutures(oSetup, oLogger);
                case ExchangeType.BitmartFutures:
                    return new BitmartFutures(oSetup, oLogger);
                case ExchangeType.BitunixFutures:
                    return new BitunixFutures(oSetup, oLogger); 
                default:
                    break;
            }
            throw new NotImplementedException();
        }
    }

}

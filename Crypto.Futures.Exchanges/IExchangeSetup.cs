using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges
{

    public enum ExchangeType
    {
        // MexcSpot,
        MexcFutures,
        //BingxSpot,
        CoinExFutures,
        BingxFutures,
        BitgetFutures,
        BitmartFutures,
        BitUnixFutures
    }

    public interface IApiKey
    {
        public ExchangeType ExchangeType { get; }
        public string ApiKey { get; }
        public string ApiSecret { get; }
    }

    public interface IExchangeSetup
    {
        public IApiKey[] ApiKeys { get; }


        public ExchangeType[] ExchangeTypes { get; }
        public string LogPath { get; }
    }
}

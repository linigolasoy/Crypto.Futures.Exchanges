using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges
{

    public enum ExchangeType
    {
        BlofinFutures,
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
        public string? ApiPassword { get; } 
    }

    public interface IMoneySetup
    {
        public decimal Money { get; }   
        public decimal Leverage { get; }
    }

    public interface IArbitrageSetup
    {
        public decimal MinimumPercent { get; }
        public int MaxOperations { get; }
    }

    public interface IExchangeSetup
    {
        public IApiKey[] ApiKeys { get; }


        public ExchangeType[] ExchangeTypes { get; }
        public string LogPath { get; }

        public IMoneySetup MoneyDefinition { get; }
        public IArbitrageSetup Arbitrage { get; }
    }
}

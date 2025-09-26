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
        ToobitFutures,
        BitunixFutures
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
        public decimal ClosePercent { get; }
        public int MaxOperations { get; }
    }

    public interface ITelegramSetup
    {
        public long ApiId { get; }
        public string ApiHash { get; }
        public string Phone { get; }
    }


    public interface IExchangeSetup
    {
        public IApiKey[] ApiKeys { get; }


        public ExchangeType[] ExchangeTypes { get; }
        public string LogPath { get; }

        public IMoneySetup MoneyDefinition { get; }
        public IArbitrageSetup Arbitrage { get; }

        public ITelegramSetup TelegramSetup { get; }
    }
}

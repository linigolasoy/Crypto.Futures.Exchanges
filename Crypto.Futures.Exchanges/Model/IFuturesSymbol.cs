using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    /// <summary>
    /// IFuturesSymbol represents a futures trading symbol on an exchange.
    /// </summary>
    public interface IFuturesSymbol
    {
        public IFuturesExchange Exchange { get; }
        public string Symbol { get; }
        public string Base { get; }
        public string Quote { get; }

        public int LeverageMax { get; }
        public int LeverageMin { get; }

        public decimal FeeMaker { get; }
        public decimal FeeTaker { get; }

        public int Decimals { get; }
        public decimal ContractSize { get; }
        public bool UseContractSize { get; }
        public int QuantityDecimals { get; }

        public decimal Minimum { get; }

        public DateTime ListDate { get; }   
    }
}

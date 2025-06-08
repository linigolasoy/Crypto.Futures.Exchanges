using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    internal class BaseSymbol : IFuturesSymbol
    {
        public BaseSymbol(IFuturesExchange oExchange, string strSymbol, string strBase, string strQuote) 
        { 
            Exchange = oExchange;
            Symbol = strSymbol;
            Base = strBase;
            Quote = strQuote;
        }
        public IFuturesExchange Exchange { get; }

        public string Symbol { get; }

        public string Base { get; }

        public string Quote { get; }

        public int LeverageMax { get; internal set; } = 1;

        public int LeverageMin { get; internal set; } = 1;

        public decimal FeeMaker { get; internal set; } = 0;

        public decimal FeeTaker { get; internal set; } = 0;

        public int Decimals { get; internal set; } = 0;

        public decimal ContractSize { get; internal set; } = 0;

        public bool UseContractSize { get; internal set; } = false;

        public int QuantityDecimals { get; internal set; } = 0;

        public decimal Minimum { get; internal set; } = 1;

        public DateTime ListDate { get; internal set; } = DateTime.MinValue;

        public override string ToString()
        {
            return $"{Symbol} ({Exchange.ExchangeType.ToString()})";
        }
    }
}

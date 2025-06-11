using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    public class BaseSymbol : IFuturesSymbol
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

        public int LeverageMax { get; set; } = 1;

        public int LeverageMin { get; set; } = 1;

        public decimal FeeMaker { get; set; } = 0;

        public decimal FeeTaker { get; set; } = 0;

        public int Decimals { get; set; } = 0;

        public decimal ContractSize { get; set; } = 0;

        public bool UseContractSize { get; set; } = false;

        public int QuantityDecimals { get; set; } = 0;

        public decimal Minimum { get; set; } = 1;

        public DateTime ListDate { get; set; } = DateTime.MinValue;

        public override string ToString()
        {
            return $"{Symbol} ({Exchange.ExchangeType.ToString()})";
        }
    }
}

using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.FundingRates
{

    public interface IFundingRateSymbolData
    {
        public IFuturesSymbol Symbol { get; }
        public IFundingRate? RateOpen { get; }

        public decimal? PriceOpen { get; }
        public DateTime? TimeOpen { get; }
        public decimal Quantity { get; }
        public decimal? PriceClose { get; }
        public DateTime? TimeClose { get; }
    }


    /// <summary>
    /// Funding rate chance interface
    /// </summary>
    public interface IFundingRateChance
    {
        public IFundingRateBot Bot { get; }

        public DateTime ChanceDate { get; } 
        public decimal PercentDifference { get; }   
        public IFundingRateSymbolData SymbolLong { get; }

        public IFundingRateSymbolData SymbolShort { get; }

        public bool IsActive { get; } 
        public decimal Pnl { get; }
        public decimal Profit { get; }
    }



}

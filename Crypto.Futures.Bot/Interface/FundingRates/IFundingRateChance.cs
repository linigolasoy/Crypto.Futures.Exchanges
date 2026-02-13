using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Interface.FundingRates
{

    /// <summary>
    /// Symbol data for funding rate chance 
    /// </summary>
    public interface IFundingRateSymbolData
    {
        public IFuturesSymbol Symbol { get; }

        public IFundingRate RateOpen { get; set; }

        public IPosition? Position { get; set; }
        /*

        public decimal? PriceOpen { get; }
        public DateTime? TimeOpen { get; }
        public decimal Quantity { get; }
        public decimal? PriceClose { get; }
        public DateTime? TimeClose { get; }
        */

    }


    /// <summary>
    /// Funding rate chance interface
    /// </summary>
    public interface IFundingRateChance
    {
        public IFundingRateBot Bot { get; }
        public int Id { get; }

        public DateTime ChanceOpenDate { get; }
        public DateTime ChanceNextFundingDate { get; }

        public DateTime LastFundingUpdate { get; set; } 
        public decimal PercentDifference { get; }   
        public IFundingRateSymbolData SymbolLong { get; }

        public IFundingRateSymbolData SymbolShort { get; }

        public string Currency { get; } 
        public bool IsActive { get; } 
        public bool NeedClose { get; set; }  
        public decimal Pnl { get; set; }
        public decimal Profit { get; }
    }



}

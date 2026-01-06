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
        public IFundingRate? RateOpen { get; }

        public decimal? PriceOpen { get; }
        public DateTime? TimeOpen { get; }
        public decimal Quantity { get; }
        public decimal? PriceClose { get; }
        public DateTime? TimeClose { get; }

        public IOrderbookPrice? OrderbookPrice { get;}
        public Task<bool> Refresh(ICommonLogger oLogger);

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

        public string Currency { get; } 
        public bool IsActive { get; } 
        public decimal Pnl { get; }
        public decimal Profit { get; }
    }



}

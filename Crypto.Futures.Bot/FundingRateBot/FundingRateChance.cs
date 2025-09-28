using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.FundingRateBot
{

    internal class FundingRateSymbolData : IFundingRateSymbolData
    {
        public FundingRateSymbolData(IFuturesSymbol? symbol, IFundingRate? rateOpen)
        {
            if( rateOpen != null )
            {
                RateOpen = rateOpen;
                symbol = rateOpen.Symbol;
            }
            else if(symbol != null)
            {
                Symbol = symbol;
                RateOpen = null;
            }
            else
            {
                throw new ArgumentException("Either symbol or rateOpen must be provided");  
            }
        }

        public IFuturesSymbol Symbol { get; }

        public IFundingRate? RateOpen { get; }

        public decimal? PriceOpen { get; set; } = null; 

        public DateTime? TimeOpen { get; set; } = null;

        public decimal Quantity { get; set; } = 0;

        public decimal? PriceClose { get; set; } = null;

        public DateTime? TimeClose { get; set; } = null;
    }

    internal class FundingRateChance : IFundingRateChance
    {
        public IFundingRateBot Bot { get; }

        public FundingRateChance(
            IFundingRateBot bot, 
            IFundingRate oRateLong,
            IFundingRate oRateShort,
            decimal nDifference
            )
        {
            Bot = bot;
            SymbolLong = new FundingRateSymbolData(oRateLong.Symbol, oRateLong);
            SymbolShort = new FundingRateSymbolData(oRateShort.Symbol, oRateShort);

            ChanceDate = oRateLong.Next < oRateShort.Next ? oRateLong.Next : oRateShort.Next;
            PercentDifference = nDifference;
        }

        public IFundingRateSymbolData SymbolLong { get; }

        public IFundingRateSymbolData SymbolShort { get; }
        public DateTime ChanceDate { get; }
        public decimal PercentDifference { get; }

        public bool IsActive { get; set; } = true;

        public decimal Pnl { get; set; } = 0;

        public decimal Profit { get; set; } = 0;
    }
}

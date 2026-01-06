using Crypto.Futures.Bot.Interface.FundingRates;
using Crypto.Futures.Exchanges;
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
                Symbol = rateOpen.Symbol;
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

        public IOrderbookPrice? OrderbookPrice { get; private set; } = null;

        /// <summary>
        /// Starts websocket or gets orderbook price    
        /// </summary>
        /// <param name="oLogger"></param>
        /// <returns></returns>
        public async Task<bool> Refresh( ICommonLogger oLogger )
        {
            if (OrderbookPrice != null) return true;
            try
            {
                if( !Symbol.Exchange.Market.Websocket.Started )
                {
                    if(!await Symbol.Exchange.Market.Websocket.Start())
                    {
                        oLogger.Error($"FundingRateSymbolData.Refresh: Cannot start websocket for exchange {Symbol.Exchange.ExchangeType.ToString()}");
                        return false;
                    }
                }
                var oSubscribe = await Symbol.Exchange.Market.Websocket.Subscribe(Symbol, Exchanges.WebsocketModel.WsMessageType.OrderbookPrice);
                if (oSubscribe == null)
                {
                    oLogger.Error($"FundingRateSymbolData.Refresh: Cannot subscribe to orderbook for symbol {Symbol.Symbol}");
                    return false;
                }
                await Task.Delay(1000);
                var oData = Symbol.Exchange.Market.Websocket.DataManager.GetData(Symbol);
                if (oData == null || oData.LastOrderbookPrice == null)
                {
                    oLogger.Error($"FundingRateSymbolData.Refresh: No orderbook data for symbol {Symbol.Symbol}");
                    return false;
                }

                OrderbookPrice = oData.LastOrderbookPrice;
                return true;
            }
            catch(Exception ex)
            {
                oLogger.Error($"FundingRateSymbolData.Refresh: Error refreshing symbol {Symbol.Symbol}: {ex.Message}", ex);
                return false;
            }   
        }

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

            Currency = oRateLong.Symbol.Base;   
            ChanceDate = oRateLong.Next < oRateShort.Next ? oRateLong.Next : oRateShort.Next;
            PercentDifference = nDifference;
        }

        public string Currency { get; }
        public IFundingRateSymbolData SymbolLong { get; }

        public IFundingRateSymbolData SymbolShort { get; }
        public DateTime ChanceDate { get; }
        public decimal PercentDifference { get; }

        public bool IsActive { get; set; } = true;

        public decimal Pnl { get; set; } = 0;

        public decimal Profit { get; set; } = 0;
    }
}

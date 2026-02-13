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
        public FundingRateSymbolData(IFuturesSymbol? symbol, IFundingRate rateOpen, IPosition? oPosition = null)
        {
            RateOpen = rateOpen;
            Symbol = rateOpen.Symbol;
            Position = oPosition;
        }

        public IFuturesSymbol Symbol { get; }

        public IFundingRate? RateOpen { get; set; } = null;
        public IPosition? Position { get; set; } = null;

        /*

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
        */
    }

    internal class FundingRateChance : IFundingRateChance
    {
        public IFundingRateBot Bot { get; }

        private static int m_nLastId = 0;
        public int Id { get; } = ++m_nLastId;



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
            ChanceNextFundingDate = oRateLong.Next < oRateShort.Next ? oRateLong.Next : oRateShort.Next;
            ChanceOpenDate = DateTime.Now;
            PercentDifference = nDifference;
            LastFundingUpdate = DateTime.Now;
        }

        public string Currency { get; }
        public IFundingRateSymbolData SymbolLong { get; }

        public IFundingRateSymbolData SymbolShort { get; }
        public DateTime ChanceOpenDate { get; }
        public DateTime ChanceNextFundingDate { get; internal set; }

        public DateTime LastFundingUpdate { get; set; }

        public decimal PercentDifference { get; internal set; }

        public bool IsActive { get; set; } = true;
        public bool NeedClose { get; set; } = false;    

        public decimal Pnl { get; set; } = 0;

        public decimal Profit { get; set; } = 0;
    }
}

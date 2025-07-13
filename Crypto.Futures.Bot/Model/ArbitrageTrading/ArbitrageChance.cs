using Crypto.Futures.Bot.Interface.Arbitrage;
using Crypto.Futures.Exchanges;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.ArbitrageTrading
{

    /// <summary>
    /// Chance implementation
    /// </summary>
    internal class ArbitrageChance : IArbitrageChance
    {

        public ArbitrageChance( IExchangeSetup oSetup, string strCurrency, IOrderbookPrice oBookLong, IOrderbookPrice oBookShort ) 
        { 
            Setup = oSetup;
            Currency = strCurrency;
            OrderbookLong = oBookLong;
            OrderbookShort = oBookShort;
        }
        public IExchangeSetup Setup { get; }

        public string Currency { get; }

        public IOrderbookPrice OrderbookLong { get; }

        public IOrderbookPrice OrderbookShort { get; }

        public decimal PriceLong { get; private set; } = 0;

        public decimal PriceShort { get; private set; } = 0;

        public decimal Percent { get; private set; } = -100;

        private static decimal m_nMaxPercent = -100.0M;


        public override string ToString()
        {
            return $"{Currency} Long [{OrderbookLong.Symbol.ToString()}] Short [{OrderbookShort.Symbol.ToString()}] Percent {Percent}%";
        }
        /// <summary>
        /// Check chance validity
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            DateTime dNow = DateTime.Now;

            double nDiffTimeLong = (dNow - OrderbookLong.Symbol.Exchange.Market.Websocket.DataManager.LastUpdate).TotalMilliseconds;
            double nDiffTimeShort = (dNow - OrderbookShort.Symbol.Exchange.Market.Websocket.DataManager.LastUpdate).TotalMilliseconds;

            // double nDiffTimeLong = (dNow - OrderbookLong.DateTime).TotalMilliseconds;
            // double nDiffTimeShort = (dNow - OrderbookShort.DateTime).TotalMilliseconds;
            if ( nDiffTimeLong > 1000 || nDiffTimeShort > 1000  ) return false;
            PriceLong = OrderbookLong.AskPrice;
            PriceShort = OrderbookShort.BidPrice;

            if( PriceLong <= 0 || PriceShort <= 0 ) return false;
            decimal nDiff = (PriceShort - PriceLong);
            Percent = Math.Round( 100.0M * nDiff / PriceLong, 2);
            if( Percent > m_nMaxPercent )
            {
                m_nMaxPercent = Percent;
            }
            if( Percent >= 10.0M || Percent < Setup.Arbitrage.MinimumPercent ) return false;
            return true;
        }
    }
}

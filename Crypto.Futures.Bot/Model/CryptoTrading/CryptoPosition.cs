using Crypto.Futures.Bot.Interface;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Bot.Model.CryptoTrading
{
    /// <summary>
    /// Crypto position implementation
    /// </summary>
    internal class CryptoPosition : ICryptoPosition
    {
        private static int m_nId = 0;
        public CryptoPosition(ICryptoTrader oTrader, IOrderbookPrice oPrice, IOrder oOrderOpen ) 
        { 
            Trader = oTrader;
            Symbol = oPrice.Symbol;
            OrderbookPrice = oPrice;
            OrderOpen = oOrderOpen;
            IsLong = (oOrderOpen.Side == ModelOrderSide.Buy);
            Id = ++m_nId;   
            Quantity = oOrderOpen.Quantity;
        }

        public int Id { get; }
        public ICryptoTrader Trader { get; }

        public IFuturesSymbol Symbol { get; }

        public IOrderbookPrice OrderbookPrice { get; }

        public bool IsLong { get; }

        public decimal LastPrice { get; private set; }
        public decimal Quantity { get; }
        public decimal Profit { get; private set; } = 0;

        public IOrder OrderOpen { get; }

        public IOrder? OrderClose { get; internal set; }

        public override string ToString()
        {
            StringBuilder oBuild = new StringBuilder();
            oBuild.Append($"Position {Id} Long({IsLong}) ");
            oBuild.Append($"on {Symbol.ToString()} qty {Quantity} price open {OrderOpen.FilledPrice} ");
            if( OrderClose != null )
            {
                oBuild.Append($" price close {OrderClose.FilledPrice} ");
                if( OrderClose.Status == ModelOrderStatus.PartiallyFilled )
                {
                    oBuild.Append($"CLOSED");
                }
            }
            return oBuild.ToString();  
        }

        /// <summary>
        /// Updates price and profit
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Update()
        {
            decimal nPrice = (IsLong ? OrderbookPrice.BidPrice : OrderbookPrice.AskPrice);
            if ( OrderClose != null && OrderClose.Status == ModelOrderStatus.Filled )
            {
                nPrice = OrderClose.FilledPrice;
            }
            if (nPrice <= 0) return false;
            decimal nFeesOpen  = ( OrderOpen.Type == ModelOrderType.Limit? Symbol.FeeMaker : Symbol.FeeTaker) * OrderOpen.FilledPrice * Quantity;
            decimal nFeeTypeClose = Symbol.FeeTaker;
            if (OrderClose != null && OrderClose.Type == ModelOrderType.Limit) nFeeTypeClose = Symbol.FeeMaker;
            decimal nFeesClose = nFeeTypeClose * nPrice * Quantity;
            LastPrice = nPrice;
            decimal nDiff = nPrice - OrderOpen.FilledPrice;
            if (!IsLong) nDiff *= -1;
            Profit = Math.Round( (nDiff * Quantity) - nFeesOpen - nFeesClose , 3);
            return true;
        }
    }
}

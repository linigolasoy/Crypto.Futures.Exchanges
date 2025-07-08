using BitMart.Net.Enums;
using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{
    internal class BitmartPositionMine: IPosition
    {
        public BitmartPositionMine(IFuturesSymbol oSymbol, BitMartPosition oJson)
        {
            Id = Guid.NewGuid().ToString();
            Symbol = oSymbol;
            CreatedAt = (oJson.OpenTime == null ? DateTime.Now: oJson.OpenTime.Value.ToLocalTime());
            UpdatedAt = (oJson.Timestamp.ToLocalTime());
            IsLong = (oJson.PositionSide == PositionSide.Long );
            IsOpen = true;
            AveragePriceOpen = (oJson.OpenAveragePrice == null? 0: oJson.OpenAveragePrice.Value);
            Quantity = (oJson.CurrentQuantity == null? 0 : oJson.CurrentQuantity.Value) * oSymbol.ContractSize;
        }


        public BitmartPositionMine(IFuturesSymbol oSymbol, BitMartFuturesUserTrade oOpenTrade, BitMartFuturesUserTrade oCloseTrade)
        {
            Id = Guid.NewGuid().ToString();
            Symbol = oSymbol;
            CreatedAt = oOpenTrade.CreateTime.ToLocalTime();
            UpdatedAt = oCloseTrade.CreateTime.ToLocalTime();
            IsLong = (oOpenTrade.Side == FuturesSide.SellCloseLong || oOpenTrade.Side == FuturesSide.BuyOpenLong);
            IsOpen = true;
            AveragePriceOpen = oOpenTrade.Price;
            Quantity = oOpenTrade.Quantity * oSymbol.ContractSize;
            PriceClose = oCloseTrade.Price;
        }

        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; }

        public bool IsLong { get; }

        public bool IsOpen { get; }

        public decimal AveragePriceOpen { get; }
        public decimal? PriceClose { get; set; } = null;

        public decimal Quantity { get; }
    }
}

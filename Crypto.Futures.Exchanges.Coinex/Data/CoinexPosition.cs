using CoinEx.Net.Enums;
using CoinEx.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Coinex.Data
{
    internal class CoinexPosition : IPosition
    {

        public CoinexPosition( IFuturesSymbol oSymbol, CoinExPosition oJson)
        {
            // Constructor logic if needed
            Id = oJson.Id.ToString();
            Symbol = oSymbol;
            CreatedAt = oJson.CreateTime.ToLocalTime();
            UpdatedAt = (oJson.UpdateTime ==null? DateTime.Now: oJson.UpdateTime.Value.ToLocalTime());
            IsLong = (oJson.Side == PositionSide.Long);
            IsOpen = true;
            AveragePriceOpen = oJson.AverageEntryPrice;
            Quantity = oJson.AthPositionQuantity;
        }
        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; }

        public bool IsLong { get; }

        public bool IsOpen { get; }

        public decimal AveragePriceOpen { get; }

        public decimal Quantity { get; }
    }
}

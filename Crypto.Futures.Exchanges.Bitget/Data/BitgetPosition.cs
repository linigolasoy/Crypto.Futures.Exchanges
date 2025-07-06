using Bitget.Net.Enums.V2;
using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    internal class BitgetPositionMine : IPosition
    {

        public BitgetPositionMine(IFuturesSymbol oSymbol, BitgetPosition oJson)
        {
            // Constructor logic if needed
            Id = Guid.NewGuid().ToString();
            Symbol = oSymbol;
            CreatedAt = oJson.UpdateTime.ToLocalTime();
            UpdatedAt = oJson.UpdateTime.ToLocalTime();
            IsLong = (oJson.PositionSide == PositionSide.Long);
            IsOpen = true;
            AveragePriceOpen = oJson.AverageOpenPrice;
            Quantity = oJson.Total;

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

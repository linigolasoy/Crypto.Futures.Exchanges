using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toobit.Net.Enums;
using Toobit.Net.Objects.Models;

namespace Crypto.Futures.Exchanges.Toobit.Data
{
    internal class ToobitPositionMine : IPosition
    {

        public ToobitPositionMine(IFuturesSymbol oSymbol, ToobitPositionUpdate oUpdate) 
        { 
            Symbol = oSymbol;
            Quantity = (decimal)oUpdate.PositionQuantity * oSymbol.ContractSize;
            IsLong = (oUpdate.PositionSide == PositionSide.Long ? true : false);
            AveragePriceOpen = oUpdate.AveragePrice;
            string strId = $"{oSymbol.Symbol}_{IsLong.ToString()}_{oUpdate.AveragePrice.ToString()}";
            Id = strId;
            IsOpen = (oUpdate.PositionQuantity > 0);
        }
        public ToobitPositionMine(IFuturesSymbol oSymbol, ToobitPosition oUpdate)
        {
            Symbol = oSymbol;
            Quantity = (decimal)oUpdate.Position * oSymbol.ContractSize;
            IsLong = (oUpdate.PositionSide == PositionSide.Long ? true : false);
            AveragePriceOpen = oUpdate.AveragePrice;
            string strId = $"{oSymbol.Symbol}_{IsLong.ToString()}_{oUpdate.AveragePrice.ToString()}";
            Id = strId;
            IsOpen = (Quantity > 0);
        }

        public string Id { get; }

        public IFuturesSymbol Symbol { get; }

        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public bool IsLong { get; }

        public bool IsOpen { get; private set; }

        public decimal AveragePriceOpen { get; }

        public decimal? PriceClose { get; set; } = null;

        public decimal Quantity { get; }

        public decimal Profit { get; set; } = 0;

        public WsMessageType MessageType { get => WsMessageType.Position; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IPosition)) return;
            IPosition oPosition = (IPosition)oMessage;
            PriceClose = oPosition.PriceClose;
            IsOpen = oPosition.IsOpen; 

        }
    }
}

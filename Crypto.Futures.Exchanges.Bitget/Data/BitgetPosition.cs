using Bitget.Net.Enums.V2;
using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;

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

        public BitgetPositionMine(IFuturesSymbol oSymbol, BitgetPositionUpdate oUpdate)
        {
            Id = oUpdate.PositionId;
            Symbol = oSymbol;
            CreatedAt = oUpdate.CreateTime.ToLocalTime();
            UpdatedAt = oUpdate.UpdateTime.ToLocalTime();
            IsLong = (oUpdate.PositionSide == PositionSide.Long);
            IsOpen = true;
            AveragePriceOpen = oUpdate.AverageOpenPrice;
            Quantity = oUpdate.Total;
            Profit = oUpdate.UnrealizedProfitAndLoss + oUpdate.RealizedProfitAndLoss;
        }

        public BitgetPositionMine(IFuturesSymbol oSymbol, BitgetPositionHistoryEntry oJson)
        {
            // Constructor logic if needed
            Id = Guid.NewGuid().ToString();
            Symbol = oSymbol;
            CreatedAt = oJson.CreateTime.ToLocalTime();
            UpdatedAt = oJson.UpdateTime.ToLocalTime(); 
            IsLong = (oJson.Side == PositionSide.Long);
            IsOpen = true;
            AveragePriceOpen = oJson.AverageOpenPrice;
            Quantity = oJson.OpenTotalPosition;
            PriceClose = oJson.AverageClosePrice;

        }
        public WsMessageType MessageType { get => WsMessageType.Position; }

        public string Id { get; }

        public IFuturesSymbol Symbol { get; }
        public decimal Profit { get; private set; } = 0;
        public DateTime CreatedAt { get; }

        public DateTime UpdatedAt { get; private set; }

        public bool IsLong { get; }

        public bool IsOpen { get; internal set; }

        public decimal AveragePriceOpen { get; }
        public decimal? PriceClose { get; set; } = null;    

        public decimal Quantity { get; }

        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IPosition)) return;
            IPosition oPosition = (IPosition)oMessage;  
            UpdatedAt = oPosition.UpdatedAt;
            IsOpen = oPosition.IsOpen;
            Profit = oPosition.Profit;
            PriceClose = oPosition.PriceClose;
        }
    }
}

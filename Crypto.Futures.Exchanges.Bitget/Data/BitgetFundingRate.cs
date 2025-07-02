using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;

namespace Crypto.Futures.Exchanges.Bitget
{




    internal class BitgetFundingRate: IFundingRate
    {
        public BitgetFundingRate(IFuturesSymbol oSymbol, BitgetCurrentFundingRate oJson) 
        {
            Symbol = oSymbol;
            Next = Util.NextFundingRate(oJson.FundingInterval);
            Rate = oJson.FundingRate;
        }

        public BitgetFundingRate(IFuturesSymbol oSymbol, BitgetFuturesTickerUpdate oTicker)
        {
            Symbol = oSymbol;
            Next = ( oTicker.NextFundingTime == null ? DateTime.MinValue : oTicker.NextFundingTime.Value.ToLocalTime());
            Rate = ( oTicker.FundingRate == null ? 0: oTicker.FundingRate.Value);
        }

        public IFuturesSymbol Symbol { get; }
        public WsMessageType MessageType { get => WsMessageType.FundingRate; }
        public DateTime Next { get; private set; }

        public decimal Rate { get; private set; }
        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is IFundingRate)) return;
            IFundingRate oFunding = (IFundingRate)oMessage;
            Next = oFunding.Next;
            Rate = oFunding.Rate;
        }

    }
}

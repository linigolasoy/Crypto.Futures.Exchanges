using BitMart.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bitmart.Data
{

    internal class BitmartFundingRate : IFundingRate
    {

        public BitmartFundingRate(IFuturesSymbol oSymbol, BitMartContract oContract)
        {
            Symbol = oSymbol;
            Next = Util.NextFundingRate( (oContract.FundingIntervalHours == null? 8 : oContract.FundingIntervalHours.Value));
            Rate = oContract.FundingRate;
        }
        public BitmartFundingRate(IFuturesSymbol oSymbol, BitMartFundingRateUpdate oUpdate)
        {
            Symbol = oSymbol;
            DateTime dNext = DateTime.MinValue;
            if(oUpdate.NextFundingTime != null )
            {
                dNext = oUpdate.NextFundingTime.Value.ToLocalTime();
            }
            Next = dNext;
            Rate = oUpdate.FundingRate;
        }
        public IFuturesSymbol Symbol { get; }
        public WsMessageType MessageType { get => WsMessageType.FundingRate; }

        public DateTime Next { get; private set; }

        public decimal Rate { get; private set; }
        public void Update(IWebsocketMessageBase oMessage)
        {
            if (!(oMessage is IFundingRate)) return;
            IFundingRate oFunding = (IFundingRate)oMessage;
            Next = oFunding.Next;
            Rate = oFunding.Rate;
        }


    }
}

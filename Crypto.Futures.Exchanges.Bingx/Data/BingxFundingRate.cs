using BingX.Net.Objects.Models;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bingx.Data
{



    internal class BingxFundingRate : IFundingRate
    {

        public BingxFundingRate(IFuturesSymbol oSymbol, BingXFundingRate oJson )
        {
            Symbol = oSymbol;
            Next = oJson.NextFundingTime.ToLocalTime();
            Rate = oJson.LastFundingRate;
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

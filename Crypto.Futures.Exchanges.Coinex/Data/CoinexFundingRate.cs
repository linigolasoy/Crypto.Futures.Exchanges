using Crypto.Futures.Exchanges.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Crypto.Futures.Exchanges.WebsocketModel;
using CoinEx.Net.Objects.Models.V2;

namespace Crypto.Futures.Exchanges.Coinex.Data
{


    internal class CoinexFundingRate : IFundingRate
    {

        public CoinexFundingRate( IFuturesSymbol oSymbol, CoinExFundingRate oJson ) 
        { 
            Symbol = oSymbol;
            Next = ( oJson.LastFundingTime == null? DateTime.MinValue : oJson.LastFundingTime.Value.ToLocalTime());
            Rate = oJson.LastFundingRate;

        }
        public CoinexFundingRate(IFuturesSymbol oSymbol, CoinExFuturesTickerUpdate oJson)
        {
            Symbol = oSymbol;
            Next = (oJson.LastFundingTime == null ? DateTime.MinValue : oJson.LastFundingTime.Value.ToLocalTime());
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

using CoinEx.Net.Objects.Models.V2;
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
using System.Xml.Linq;

namespace Crypto.Futures.Exchanges.Coinex.Data
{

    internal class CoinexTicker: ITicker
    {
        public CoinexTicker(IFuturesSymbol oSymbol, CoinExFuturesTicker oJson )
        {
            Symbol = oSymbol;
            DateTime = DateTime.Now;
            LastPrice = oJson.LastPrice;
            AskPrice = LastPrice;
            BidPrice = LastPrice;
            AskVolume = 0;
            BidVolume = 0;
        }
        public DateTime DateTime { get; private set; }

        public decimal LastPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidVolume { get; private set; }
        public void Update(IWebsocketMessage oMessage)
        {
            if (!(oMessage is ITicker)) return;
            ITicker oTicker = (ITicker)oMessage;
            DateTime = oTicker.DateTime;
            AskPrice = oTicker.AskPrice;
            BidPrice = oTicker.BidPrice;
            AskVolume = oTicker.AskVolume;
            BidVolume = oTicker.BidVolume;
            LastPrice = oTicker.LastPrice;

        }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

    }
}

using Bitget.Net.Objects.Models.V2;
using Crypto.Futures.Exchanges.Model;
using Crypto.Futures.Exchanges.WebsocketModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Bitget.Data
{

    internal class BitgetTicker: ITicker
    {
        public BitgetTicker(IFuturesSymbol oSymbol, BitgetFuturesTicker oJson) 
        { 
            Symbol = oSymbol;
            DateTime = oJson.Timestamp.ToLocalTime();
            LastPrice = oJson.LastPrice;
            BidPrice = (oJson.BestBidPrice == null ? LastPrice : oJson.BestBidPrice.Value);
            AskPrice = (oJson.BestAskPrice == null ? LastPrice : oJson.BestAskPrice.Value);
            AskVolume = (oJson.BestAskQuantity == null ? 0 : oJson.BestAskQuantity.Value) * oSymbol.ContractSize;
            BidVolume = (oJson.BestBidQuantity == null ? 0 : oJson.BestBidQuantity.Value) * oSymbol.ContractSize;

        }
        public DateTime DateTime { get; private set; }

        public decimal LastPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal BidPrice { get; private set; }

        public decimal AskVolume { get; private set; }

        public decimal BidVolume { get; private set; }

        public WsMessageType MessageType { get => WsMessageType.Ticker; }

        public IFuturesSymbol Symbol { get; }

        public void Update(IWebsocketMessageBase oMessage)
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



    }
}

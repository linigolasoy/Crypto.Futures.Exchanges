using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Crypto.Futures.Exchanges.Bingx.Data
{
    /*
    internal class BingxBarJson
    {
        [JsonProperty("open")]
        public decimal Open { get; set; } = 0;
        [JsonProperty("close")]
        public decimal Close { get; set; } = 0;
        [JsonProperty("high")]
        public decimal High { get; set; } = 0;
        [JsonProperty("low")]
        public decimal Low { get; set; } = 0;
        [JsonProperty("volume")]
        public decimal Volume { get; set; } = 0;
        [JsonProperty("time")]
        public long Time { get; set; } = 0;
    }
    internal class BingxBar: FuturesBar
    {
        public BingxBar(
                IFuturesSymbol oSymbol,
                BarTimeframe eFrame,
                DateTime dDate,
                decimal nOpen,
                decimal nHigh,
                decimal nLow,
                decimal nClose,
                decimal nVolume
            ): base(oSymbol, eFrame)
        {
            DateTime = dDate;
            Open = nOpen;
            High = nHigh;
            Low = nLow;
            Close = nClose;
            Volume = nVolume;

        }

        public static IBar? Parse( IFuturesExchange oExchange, IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oJson )
        {
            if( oJson == null ) return null;
            var oParsed = oJson.ToObject<BingxBarJson>();   
            if( oParsed == null ) return null;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(oParsed.Time);
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            return new BingxBar(oSymbol, eFrame, dDate, oParsed.Open, oParsed.High, oParsed.Low, oParsed.Close, oParsed.Volume);    
        }
    }
    */
}

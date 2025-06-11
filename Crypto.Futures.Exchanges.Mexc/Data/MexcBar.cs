using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Mexc.Data
{


    internal class MexcBarData
    {
        [JsonProperty("open")]
        public List<decimal> Open { get; set; } = new List<decimal>();
        [JsonProperty("close")]
        public List<decimal> Close { get; set; } = new List<decimal>();
        [JsonProperty("high")]
        public List<decimal> High { get; set; } = new List<decimal>();
        [JsonProperty("low")]
        public List<decimal> Low { get; set; } = new List<decimal>();
        [JsonProperty("vol")]
        public List<decimal> Volume { get; set; } = new List<decimal>();
        [JsonProperty("time")]
        public List<long> Times { get; set; } = new List<long>();
    }
    internal class MexcBar : FuturesBar
    {
        public MexcBar(
                IFuturesSymbol oSymbol,
                BarTimeframe eFrame,
                DateTime dDate,
                decimal nOpen,
                decimal nHigh,
                decimal nLow,
                decimal nClose,
                decimal nVolume
            ): base( oSymbol, eFrame )
        {
            DateTime = dDate;
            Open = nOpen;
            High = nHigh; 
            Low = nLow;
            Close = nClose;
            Volume = nVolume;

        }

        public static IBar[]? Parse( IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oToken )
        {
            if( oToken == null ) return null;
            var oJson = oToken.ToObject<MexcBarData>();
            if( oJson == null ) return null;
            List<IBar> aResult = new List<IBar>();  
            for (int i = 0; i < oJson.Times.Count; i++)
            {
                DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeSeconds(oJson.Times[i]);
                DateTime dDate = oOffset.DateTime.ToLocalTime();

                aResult.Add(new MexcBar(
                        oSymbol,
                        eFrame,
                        dDate,
                        oJson.Open[i],
                        oJson.High[i],
                        oJson.Low[i],
                        oJson.Close[i],
                        oJson.Volume[i]
                    ));
            }

            return aResult.OrderBy(p=> p.DateTime).ToArray();
        }
    }
}

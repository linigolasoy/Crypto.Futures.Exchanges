using Crypto.Futures.Exchanges.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Bitget.Data
{
    internal class BitgetBar : FuturesBar
    {

        public BitgetBar(
                IFuturesSymbol oSymbol,
                BarTimeframe eFrame
            ) : base(oSymbol, eFrame)
        {
        }
    

        public static IBar? Parse( IFuturesExchange oExchange, IFuturesSymbol oSymbol, BarTimeframe eFrame, JToken? oToken )
        {
            if( oToken == null ) return null;
            if( !(oToken is JArray)) return null;
            JArray oArray = (JArray)oToken;
            if( oArray.Count < 7 ) return null;
            DateTimeOffset oOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(oArray[0].ToString()));
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            BitgetBar oBar = new BitgetBar(oSymbol, eFrame);
            oBar.DateTime = dDate;
            oBar.Open = decimal.Parse(oArray[1].ToString(), CultureInfo.InvariantCulture);
            oBar.High = decimal.Parse(oArray[2].ToString(), CultureInfo.InvariantCulture);
            oBar.Low = decimal.Parse(oArray[3].ToString(), CultureInfo.InvariantCulture);
            oBar.Close = decimal.Parse(oArray[4].ToString(), CultureInfo.InvariantCulture);
            oBar.Volume = decimal.Parse(oArray[5].ToString(), CultureInfo.InvariantCulture);

            return oBar;
        }
    }
}

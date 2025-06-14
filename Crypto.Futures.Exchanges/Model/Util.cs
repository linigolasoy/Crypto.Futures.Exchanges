using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Model
{
    public class Util
    {

        public static DateTime NextFundingRate( int nHourInterval )
        {
            DateTime dNow = DateTime.UtcNow;
            int nHour = dNow.Hour + 1;
            while( nHour % nHourInterval != 0 ) nHour++;
            bool bAddDays = false;
            if( nHour >= 24 )
            {
                nHour -= 24;
                bAddDays = true;
            }
            DateTime dNext = new DateTime(dNow.Year, dNow.Month, dNow.Day, nHour, 0, 0, DateTimeKind.Utc);
            if( bAddDays ) dNext = dNext.AddDays(1);
            return dNext.ToLocalTime(); 
        }


        public static DateTime FromUnixTimestamp( string strTime, bool bMillis )
        {
            return FromUnixTimestamp( long.Parse(strTime), bMillis );
        }

        public static DateTime FromUnixTimestamp( long nTime, bool bMillis)
        {
            DateTimeOffset oOffset = (bMillis ? DateTimeOffset.FromUnixTimeMilliseconds(nTime) : DateTimeOffset.FromUnixTimeSeconds(nTime));
            DateTime dDate = oOffset.DateTime.ToLocalTime();
            return dDate;   
        }

        public static long ToUnixTimestamp(DateTime d, bool bMillis )
        {
            DateTimeOffset oOffset = new DateTimeOffset(d.ToUniversalTime());
            return (bMillis? oOffset.ToUnixTimeMilliseconds(): oOffset.ToUnixTimeSeconds());
        }

        public static string EncodePayLoadHmac( string strPayLoad, IApiKey oApiKey, bool bHex )
        {
            byte[] aKey = Encoding.UTF8.GetBytes(oApiKey.ApiSecret);
            HMACSHA256 oHmac = new HMACSHA256(aKey);
            byte[] aBuffer = Encoding.UTF8.GetBytes(strPayLoad);
            byte[] aHash = oHmac.ComputeHash(aBuffer);
            if( bHex )
            {
                string strHex = BitConverter.ToString(aHash).Replace("-", string.Empty).ToLower();
                return strHex;
            }
            string strSigned = Convert.ToBase64String(aHash);

            return strSigned;
        }
    }
}

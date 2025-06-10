using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Crypto.Futures.Exchanges.Rest
{
    internal class CryptoRestError : ICryptoErrorCode
    {
        private CryptoRestError(HttpStatusCode oCode)
        {
            HttpCode = oCode;
            ErrorCode = ((int)oCode);
            Message = oCode.ToString();
        }
        private CryptoRestError(int nError, string strMessage)
        {
            HttpCode = HttpStatusCode.OK;
            ErrorCode = nError;
            Message = strMessage;
        }

        public HttpStatusCode HttpCode { get; }

        public int ErrorCode { get; }

        public string? Message { get; }

        public static ICryptoErrorCode? Create(HttpResponseMessage oResponse)
        {
            if (oResponse.IsSuccessStatusCode) return null;
            return new CryptoRestError(oResponse.StatusCode);
        }
        public static ICryptoErrorCode? Create(int nError, string strMessage)
        {
            return new CryptoRestError(nError, strMessage);
        }

        public static ICryptoErrorCode Create( Exception ex )
        {
            return new CryptoRestError(-1, ex.Message);
        }
    }
}

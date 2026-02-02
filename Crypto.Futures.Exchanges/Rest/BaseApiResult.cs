using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    public class BaseApiResult<T> : IApiResult<T>
    {
        internal BaseApiResult(HttpStatusCode eStatusCode, string? strErrorMessage)
        {
            Success = false;
            StatusCode = eStatusCode;
            ErrorMessage = strErrorMessage;
        }

        internal BaseApiResult(T data)
        {
            Success = true;
            Data = data;
            StatusCode = HttpStatusCode.OK;
        }

        internal BaseApiResult(IApiResult<string> oRequestResult)
        {
            Success = oRequestResult.Success;
            StatusCode = oRequestResult.StatusCode;
            ErrorMessage = oRequestResult.ErrorMessage;
        }

        // public static
        public bool Success { get; }

        public HttpStatusCode StatusCode { get; }

        public T? Data { get; }

        public string? ErrorMessage { get; }

        public static IApiResult<T> CreateSuccessResult(T data)
        {
            return new BaseApiResult<T>(data);
        }
        public static IApiResult<T> CreateFromResponse(HttpResponseMessage oMsg)
        {
            return new BaseApiResult<T>(oMsg.StatusCode, "Http error");
        }

        public static IApiResult<T> CreateFromException(Exception ex)
        {
            return new BaseApiResult<T>(HttpStatusCode.Unused, ex.Message);
        }


        public static IApiResult<T> CloneErrors(IApiResult<string> oResultString)
        {
            return new BaseApiResult<T>(oResultString);
        }

    }
}

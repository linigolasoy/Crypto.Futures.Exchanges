using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{

    /// <summary>
    /// Represents a generic API result interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IApiResult<T>
    {
        public bool Success { get; }
        public HttpStatusCode StatusCode { get; }
        public T? Data { get; }
        public string? ErrorMessage { get; }
    }

    /// <summary>
    /// Represents a generic API caller interface.  
    /// </summary>
    public interface IApiCaller
    {

        public Dictionary<string, string> Headers { get; }

        public void SetHeader(string strKey, string strValue);

        public string UrlBase { get; }
        public Task<IApiResult<string>> GetAsync(string strEntryPoint, Dictionary<string, string>? aParameters = null);
        public Task<IApiResult<string>> PostAsync(string strEntryPoint, string strBody);

        public Task<IApiResult<string>> PostAsync<T>(string strEntryPoint, T oBody);

    }
}

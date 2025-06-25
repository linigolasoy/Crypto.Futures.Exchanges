using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    // public delegate void RequestHeaderDelegate( HttpRequestMessage oRequest, IApiKey oApiKey );


    /// <summary>
    /// Rest client interface
    /// </summary>
    /// 
    public interface ICryptoRestClient
    {
        public IApiKey ApiKey { get; }
        public string BaseUrl { get; }
        public ICryptoRestParser Parser { get; }

        public Func<HttpMethod, string, Dictionary<string, string>?, string?, HttpRequestMessage? >? RequestEvaluator { get; }   

        // public event RequestHeaderDelegate? OnRequestHeader;
        public Task<ICryptoRestResult<T>> DoGetParams<T>(
            string strEndpoint, 
            Func<JToken?, T> oParserAction, 
            Dictionary<string, string>? aParameters = null);

        public Task<ICryptoRestResult<T[]>> DoGetArrayParams<T>(
            string strEndpoint, 
            string? strField, 
            Func<JToken, T> oParserAction, 
            Dictionary<string, string>? aParameters = null);


        public Task<ICryptoRestResult<bool>> DoPostParams<T>(
            string strEndpoint,
            T oData);
    }
}

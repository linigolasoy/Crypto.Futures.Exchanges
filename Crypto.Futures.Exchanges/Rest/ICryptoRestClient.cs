using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Futures.Exchanges.Rest
{
    /// <summary>
    /// Rest client interface
    /// </summary>
    public interface ICryptoRestClient
    {
        public string BaseUrl { get; }
        public ICryptoRestParser Parser { get; }

        public Task<ICryptoRestResult<T>> DoGet<T>(string strEndpoint, Func<JToken?, T> oParserAction, Dictionary<string, Object>? aParameters = null);
        public Task<ICryptoRestResult<T[]>> DoGetArray<T>(string strEndpoint, string? strField, Func<JToken, T> oParserAction, Dictionary<string, Object>? aParameters = null);
    }
}
